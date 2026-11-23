using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    public class ReservacionService : IReservacionService
    {
        private readonly DbContextBank dbContext;
        private readonly IPrecioService precioService;
        private readonly ITipoCambioService tipoCambioService;

        public ReservacionService(DbContextBank pContext, IPrecioService pPrecioService, ITipoCambioService pTipoCambioService)
        {
            this.dbContext = pContext;
            this.precioService = pPrecioService;
            this.tipoCambioService = pTipoCambioService;
        }

        public async Task<CotizacionDTO> CotizarAsync(CotizacionRequestDTO request, CancellationToken cancellationToken = default)
        {
            // Se valida la forma de pago antes de consultar servicios externos
            PrecioService.NormalizarFormaPago(request.FormaPago);

            Paquete paquete = await this.ObtenerPaqueteActivoAsync(request.PaqueteId);
            TipoCambioDTO tipoCambio = await this.tipoCambioService.ObtenerVentaAsync(cancellationToken);

            return this.precioService.Calcular(paquete, request.CantidadPersonas, request.CantidadNoches,
                request.FormaPago, tipoCambio.Venta);
        }

        public async Task<List<ReservacionResponseDTO>> ListarAsync()
        {
            List<Reservacion> reservaciones = await this.dbContext.Reservaciones
                .AsNoTracking()
                .Include(r => r.Cliente)
                .Include(r => r.Paquete)
                .Where(r => r.Activo)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return reservaciones.Select(r => Mapear(r)).ToList();
        }

        public async Task<ReservacionResponseDTO> BuscarPorIdAsync(int id)
        {
            Reservacion reservacion = await this.dbContext.Reservaciones
                .AsNoTracking()
                .Include(r => r.Cliente)
                .Include(r => r.Paquete)
                .FirstOrDefaultAsync(r => r.Id == id && r.Activo)
                ?? throw new KeyNotFoundException("La reservación no existe.");

            return Mapear(reservacion);
        }

        public async Task<ReservacionResponseDTO> CrearAsync(ReservacionDTO request, int usuarioId, CancellationToken cancellationToken = default)
        {
            // 1. Validaciones y cálculo, antes de abrir la transacción
            var pago = ValidarPago(request.FormaPago, request.NumeroCheque, request.Banco);

            Cliente cliente = await this.ObtenerClienteActivoAsync(request.ClienteId);
            Paquete paquete = await this.ObtenerPaqueteActivoAsync(request.PaqueteId);

            TipoCambioDTO tipoCambio = await this.tipoCambioService.ObtenerVentaAsync(cancellationToken);
            CotizacionDTO calculo = this.precioService.Calcular(paquete, request.CantidadPersonas,
                request.CantidadNoches, pago.Forma, tipoCambio.Venta);

            // 2. Guardado dentro de una transacción. La estrategia de ejecución permite que
            //    funcione igual si más adelante se activan los reintentos de EF Core.
            //    Aquí también deberá entrar el registro de auditoría cuando se defina.
            Reservacion reservacion = null!;
            var estrategia = this.dbContext.Database.CreateExecutionStrategy();

            await estrategia.ExecuteAsync(async () =>
            {
                this.dbContext.ChangeTracker.Clear();
                await using var transaccion = await this.dbContext.Database.BeginTransactionAsync(cancellationToken);

                reservacion = new Reservacion
                {
                    ClienteId = request.ClienteId,
                    UsuarioId = usuarioId,
                    NumeroCheque = pago.NumeroCheque,
                    Banco = pago.Banco,
                    CorreoEnviado = false,
                    Activo = true
                };
                AplicarCalculo(reservacion, calculo);

                this.dbContext.Reservaciones.Add(reservacion);
                await this.dbContext.SaveChangesAsync(cancellationToken);
                await transaccion.CommitAsync(cancellationToken);
            });

            return Mapear(reservacion, cliente.NombreCompleto, cliente.Cedula, paquete.Nombre);
        }

        public async Task<ReservacionResponseDTO> EditarAsync(int id, ReservacionDTO request, CancellationToken cancellationToken = default)
        {
            var pago = ValidarPago(request.FormaPago, request.NumeroCheque, request.Banco);

            Reservacion actual = await this.dbContext.Reservaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.Activo, cancellationToken)
                ?? throw new KeyNotFoundException("La reservación no existe.");

            // Si el cliente no cambia, se acepta aunque haya sido desactivado después
            bool cambiaCliente = request.ClienteId != actual.ClienteId;
            Cliente cliente = cambiaCliente
                ? await this.ObtenerClienteActivoAsync(request.ClienteId)
                : await this.dbContext.Clientes.AsNoTracking().FirstAsync(c => c.Id == actual.ClienteId, cancellationToken);

            // Solo se recalcula si cambia algo que afecta el precio
            bool cambiaCalculo = request.PaqueteId != actual.PaqueteId
                || request.CantidadPersonas != actual.CantidadPersonas
                || request.CantidadNoches != actual.CantidadNoches
                || pago.Forma != actual.FormaPago;

            CotizacionDTO? calculo = null;
            Paquete paquete;

            if (cambiaCalculo)
            {
                paquete = await this.ObtenerPaqueteActivoAsync(request.PaqueteId);
                TipoCambioDTO tipoCambio = await this.tipoCambioService.ObtenerVentaAsync(cancellationToken);
                calculo = this.precioService.Calcular(paquete, request.CantidadPersonas,
                    request.CantidadNoches, pago.Forma, tipoCambio.Venta);
            }
            else
            {
                paquete = await this.dbContext.Paquetes.AsNoTracking().FirstAsync(p => p.Id == actual.PaqueteId, cancellationToken);
            }

            Reservacion editada = null!;
            var estrategia = this.dbContext.Database.CreateExecutionStrategy();

            await estrategia.ExecuteAsync(async () =>
            {
                this.dbContext.ChangeTracker.Clear();
                await using var transaccion = await this.dbContext.Database.BeginTransactionAsync(cancellationToken);

                editada = await this.dbContext.Reservaciones
                    .FirstOrDefaultAsync(r => r.Id == id && r.Activo, cancellationToken)
                    ?? throw new KeyNotFoundException("La reservación no existe.");

                editada.ClienteId = request.ClienteId;
                editada.FormaPago = pago.Forma;
                editada.NumeroCheque = pago.NumeroCheque;
                editada.Banco = pago.Banco;

                if (calculo != null)
                {
                    AplicarCalculo(editada, calculo);
                }

                // Si cambió el cliente o los montos, el correo enviado ya no corresponde
                if (calculo != null || cambiaCliente)
                {
                    editada.CorreoEnviado = false;
                    editada.FechaCorreo = null;
                }

                await this.dbContext.SaveChangesAsync(cancellationToken);
                await transaccion.CommitAsync(cancellationToken);
            });

            return Mapear(editada, cliente.NombreCompleto, cliente.Cedula, paquete.Nombre);
        }

        public async Task EliminarAsync(int id)
        {
            Reservacion reservacion = await this.dbContext.Reservaciones
                .FirstOrDefaultAsync(r => r.Id == id && r.Activo)
                ?? throw new KeyNotFoundException("La reservación no existe.");

            reservacion.Activo = false;
            await this.dbContext.SaveChangesAsync();
        }

        // ---------- Auxiliares ----------

        private async Task<Cliente> ObtenerClienteActivoAsync(int id)
        {
            return await this.dbContext.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo)
                ?? throw new ArgumentException("El cliente no existe o está inactivo.");
        }

        private async Task<Paquete> ObtenerPaqueteActivoAsync(int id)
        {
            return await this.dbContext.Paquetes.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo)
                ?? throw new ArgumentException("El paquete no existe o está inactivo.");
        }

        /// <summary>
        /// Normaliza la forma de pago y valida cheque y banco: son obligatorios solo con Cheque;
        /// con cualquier otra forma de pago se descartan.
        /// </summary>
        private static (string Forma, string? NumeroCheque, string? Banco) ValidarPago(string formaPago, string? numeroCheque, string? banco)
        {
            string forma = PrecioService.NormalizarFormaPago(formaPago);

            if (forma != PrecioService.Cheque)
            {
                return (forma, null, null);
            }

            if (string.IsNullOrWhiteSpace(numeroCheque))
            {
                throw new ArgumentException("El número de cheque es obligatorio cuando la forma de pago es Cheque.");
            }

            if (string.IsNullOrWhiteSpace(banco))
            {
                throw new ArgumentException("El banco es obligatorio cuando la forma de pago es Cheque.");
            }

            return (forma, numeroCheque.Trim(), banco.Trim());
        }

        /// <summary>Copia el cálculo (y los valores congelados del paquete) a la reservación.</summary>
        private static void AplicarCalculo(Reservacion reservacion, CotizacionDTO calculo)
        {
            reservacion.PaqueteId = calculo.PaqueteId;
            reservacion.CantidadPersonas = calculo.CantidadPersonas;
            reservacion.CantidadNoches = calculo.CantidadNoches;
            reservacion.FormaPago = calculo.FormaPago;
            reservacion.PrecioPersonaNoche = calculo.PrecioPersonaNoche;
            reservacion.PorcentajePrima = calculo.PorcentajePrima;
            reservacion.CantidadMensualidades = calculo.CantidadMensualidades;
            reservacion.Subtotal = calculo.Subtotal;
            reservacion.PorcentajeDescuento = calculo.PorcentajeDescuento;
            reservacion.MontoDescuento = calculo.MontoDescuento;
            reservacion.PorcentajeIva = calculo.PorcentajeIva;
            reservacion.MontoIva = calculo.MontoIva;
            reservacion.TotalUsd = calculo.TotalUsd;
            reservacion.TipoCambio = calculo.TipoCambio;
            reservacion.TotalColones = calculo.TotalColones;
            reservacion.MontoPrima = calculo.MontoPrima;
            reservacion.Saldo = calculo.Saldo;
            reservacion.MontoMensualidad = calculo.MontoMensualidad;
        }

        // Usa las propiedades de navegación (hay que cargarlas con Include)
        private static ReservacionResponseDTO Mapear(Reservacion r)
        {
            return Mapear(r, r.Cliente?.NombreCompleto ?? string.Empty,
                r.Cliente?.Cedula ?? string.Empty, r.Paquete?.Nombre ?? string.Empty);
        }

        private static ReservacionResponseDTO Mapear(Reservacion r, string clienteNombre, string clienteCedula, string paqueteNombre)
        {
            return new ReservacionResponseDTO
            {
                Id = r.Id,
                ClienteId = r.ClienteId,
                ClienteNombre = clienteNombre,
                ClienteCedula = clienteCedula,
                UsuarioId = r.UsuarioId,
                PaqueteId = r.PaqueteId,
                PaqueteNombre = paqueteNombre,
                CantidadPersonas = r.CantidadPersonas,
                CantidadNoches = r.CantidadNoches,
                FormaPago = r.FormaPago,
                NumeroCheque = r.NumeroCheque,
                Banco = r.Banco,
                PrecioPersonaNoche = r.PrecioPersonaNoche,
                PorcentajePrima = r.PorcentajePrima,
                CantidadMensualidades = r.CantidadMensualidades,
                Subtotal = r.Subtotal,
                PorcentajeDescuento = r.PorcentajeDescuento,
                MontoDescuento = r.MontoDescuento,
                PorcentajeIva = r.PorcentajeIva,
                MontoIva = r.MontoIva,
                TotalUsd = r.TotalUsd,
                TipoCambio = r.TipoCambio,
                TotalColones = r.TotalColones,
                MontoPrima = r.MontoPrima,
                Saldo = r.Saldo,
                MontoMensualidad = r.MontoMensualidad,
                CorreoEnviado = r.CorreoEnviado,
                FechaCorreo = r.FechaCorreo,
                FechaCreacion = r.FechaCreacion,
                Activo = r.Activo
            };
        }
    }
}
