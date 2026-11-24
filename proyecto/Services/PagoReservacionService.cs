
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    public class PagoReservacionService : IPagoReservacionService
    {
        private readonly DbContextBank db;
        private readonly ITipoCambioService tipoCambioService;

        public PagoReservacionService(
            DbContextBank db,
            ITipoCambioService tipoCambioService)
        {
            this.db = db;
            this.tipoCambioService = tipoCambioService;
        }

        public async Task<PagoResponseDTO> AplicarPagoAsync(
            PagoRequestDTO request,
            int usuarioId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new InvalidOperationException("No se recibieron los datos del pago.");

            string moneda = request.Moneda.Trim().ToUpper();

            if (moneda != "USD" && moneda != "CRC")
                throw new InvalidOperationException("La moneda debe ser USD o CRC.");

            if (request.Monto <= 0)
                throw new InvalidOperationException("El monto debe ser mayor que cero.");

            // Evita que dos pagos simultáneos apliquen la misma mensualidad.
            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken
            );

            var reservacion = await db.Reservaciones
                .FirstOrDefaultAsync(
                    r => r.Id == request.ReservacionId && r.Activo,
                    cancellationToken
                );

            if (reservacion == null)
                throw new InvalidOperationException("La reservación no existe o está inactiva.");

            if (reservacion.Saldo <= 0)
                throw new InvalidOperationException("La reservación no tiene saldo pendiente.");

            if (reservacion.MontoMensualidad <= 0 ||
                reservacion.CantidadMensualidades <= 0)
                throw new InvalidOperationException("La reservación no tiene una mensualidad válida.");

            // Verificar que la moneda no haya sido bloqueada anteriormente.
            if (!string.IsNullOrEmpty(reservacion.MonedaPago) &&
                reservacion.MonedaPago != moneda)
            {
                throw new InvalidOperationException(
                    $"Esta reservación ya tiene pagos en {reservacion.MonedaPago}. No se puede cambiar la moneda."
                );
            }

            // Determinar cuál es la siguiente mensualidad.
            int pagosRealizados = await db.PagosReservacion
                .CountAsync(
                    p => p.ReservacionId == reservacion.Id,
                    cancellationToken
                );

            int siguienteMensualidad = pagosRealizados + 1;

            if (siguienteMensualidad > reservacion.CantidadMensualidades)
                throw new InvalidOperationException("Todas las mensualidades ya fueron registradas.");

            if (request.NumeroMensualidad != siguienteMensualidad)
                throw new InvalidOperationException(
                    $"La siguiente mensualidad pendiente es la número {siguienteMensualidad}."
                );

            // El saldo y la mensualidad de la reservación están expresados en USD.
            decimal montoEsperadoUsd = Math.Min(
                reservacion.MontoMensualidad,
                reservacion.Saldo
            );

            decimal tipoCambio = 1m;
            decimal montoEsperado;

            if (moneda == "CRC")
            {
                var cambio = await tipoCambioService.ObtenerVentaAsync(cancellationToken);

                tipoCambio = cambio.Venta;

                if (tipoCambio <= 0)
                    throw new InvalidOperationException("El tipo de cambio no es válido.");

                montoEsperado = Math.Round(
                    montoEsperadoUsd * tipoCambio,
                    2,
                    MidpointRounding.AwayFromZero
                );
            }
            else
            {
                montoEsperado = montoEsperadoUsd;
            }

            // No permitir pagos menores ni mayores a la mensualidad correspondiente.
            if (Math.Round(request.Monto, 2) != montoEsperado)
            {
                throw new InvalidOperationException(
                    $"El monto correcto para esta mensualidad es {montoEsperado:N2} {moneda}."
                );
            }

            // Crear el registro del pago.
            var pago = new PagoReservacion
            {
                ReservacionId = reservacion.Id,
                UsuarioId = usuarioId,
                NumeroMensualidad = siguienteMensualidad,
                Monto = montoEsperado,
                Moneda = moneda,
                TipoCambioAplicado = tipoCambio,
                MontoUsd = montoEsperadoUsd,
                FechaPago = DateTime.UtcNow
            };

            // Bloquear la moneda con el primer pago.
            if (string.IsNullOrEmpty(reservacion.MonedaPago))
                reservacion.MonedaPago = moneda;

            // Actualizar el saldo de la reservación.
            reservacion.Saldo = Math.Max(
                0,
                Math.Round(reservacion.Saldo - montoEsperadoUsd, 2)
            );

            db.PagosReservacion.Add(pago);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PagoResponseDTO
            {
                Id = pago.Id,
                ReservacionId = pago.ReservacionId,
                NumeroMensualidad = pago.NumeroMensualidad,
                Monto = pago.Monto,
                Moneda = pago.Moneda,
                TipoCambioAplicado = pago.TipoCambioAplicado,
                MontoUsd = pago.MontoUsd,
                FechaPago = pago.FechaPago,
                UsuarioId = pago.UsuarioId,
                SaldoRestante = reservacion.Saldo
            };
        }

        public async Task<List<PagoResponseDTO>> ObtenerHistorialAsync(
            int reservacionId,
            CancellationToken cancellationToken = default)
        {
            var pagos = await db.PagosReservacion
                .Where(p => p.ReservacionId == reservacionId)
                .OrderBy(p => p.NumeroMensualidad)
                .Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    ReservacionId = p.ReservacionId,
                    NumeroMensualidad = p.NumeroMensualidad,
                    Monto = p.Monto,
                    Moneda = p.Moneda,
                    TipoCambioAplicado = p.TipoCambioAplicado,
                    MontoUsd = p.MontoUsd,
                    FechaPago = p.FechaPago,
                    UsuarioId = p.UsuarioId
                })
                .ToListAsync(cancellationToken);

            return pagos;
        }

        public async Task<List<PagoResponseDTO>> ObtenerHistorialGeneralAsync(
            CancellationToken cancellationToken = default)
        {
            var pagos = await db.PagosReservacion
                .OrderBy(p => p.ReservacionId)
                .ThenBy(p => p.NumeroMensualidad)
                .Select(p => new PagoResponseDTO
                {
                    Id = p.Id,
                    ReservacionId = p.ReservacionId,
                    NumeroMensualidad = p.NumeroMensualidad,
                    Monto = p.Monto,
                    Moneda = p.Moneda,
                    TipoCambioAplicado = p.TipoCambioAplicado,
                    MontoUsd = p.MontoUsd,
                    FechaPago = p.FechaPago,
                    UsuarioId = p.UsuarioId
                })
                .ToListAsync(cancellationToken);

            return pagos;
        }
    }
}
