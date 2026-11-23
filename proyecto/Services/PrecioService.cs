using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    /// <summary>
    /// Reglas de precio del proyecto, en este orden:
    /// subtotal -> descuento (solo efectivo) -> IVA sobre el monto rebajado ->
    /// total USD -> total en colones -> prima, saldo y mensualidad (en USD).
    /// En efectivo los totales, la prima, el saldo y la mensualidad se redondean
    /// a enteros; en los demás casos, a dos decimales.
    /// </summary>
    public class PrecioService : IPrecioService
    {
        public const decimal PorcentajeIva = 13m;

        public const string Efectivo = "Efectivo";
        public const string Cheque = "Cheque";
        public const string Tarjeta = "Tarjeta";

        private static readonly string[] FormasPagoValidas = { Efectivo, Cheque, Tarjeta };

        public CotizacionDTO Calcular(Paquete paquete, int cantidadPersonas, int cantidadNoches, string formaPago, decimal tipoCambio)
        {
            if (paquete == null)
                throw new ArgumentException("El paquete no existe.");
            if (cantidadPersonas < 1)
                throw new ArgumentException("La cantidad de personas debe ser al menos 1.");
            if (cantidadNoches < 1)
                throw new ArgumentException("La cantidad de noches debe ser al menos 1.");
            if (tipoCambio <= 0)
                throw new ArgumentException("El tipo de cambio debe ser mayor que cero.");
            if (paquete.PrecioPersonaNoche <= 0)
                throw new ArgumentException("El precio del paquete debe ser mayor que cero.");
            if (paquete.PorcentajePrima < 0 || paquete.PorcentajePrima > 100)
                throw new ArgumentException("El porcentaje de prima del paquete debe estar entre 0 y 100.");
            if (paquete.CantidadMensualidades < 1)
                throw new ArgumentException("La cantidad de mensualidades del paquete debe ser al menos 1.");

            string forma = NormalizarFormaPago(formaPago);
            bool esEfectivo = forma == Efectivo;

            // 1. Subtotal en dólares
            decimal subtotal = Redondear(paquete.PrecioPersonaNoche * cantidadPersonas * cantidadNoches, false);

            // 2. Descuento por noches, solo si paga en efectivo
            decimal porcentajeDescuento = esEfectivo ? ObtenerPorcentajeDescuento(cantidadNoches) : 0m;
            decimal montoDescuento = Redondear(subtotal * porcentajeDescuento / 100m, false);
            decimal montoRebajado = subtotal - montoDescuento;

            // 3. IVA sobre el monto ya rebajado
            decimal montoIva = Redondear(montoRebajado * PorcentajeIva / 100m, false);

            // 4. Totales: primero USD, luego colones a partir del USD ya redondeado
            decimal totalUsd = Redondear(montoRebajado + montoIva, esEfectivo);
            decimal totalColones = Redondear(totalUsd * tipoCambio, esEfectivo);

            // 5. Prima, saldo y mensualidad (en USD)
            decimal montoPrima = Redondear(totalUsd * paquete.PorcentajePrima / 100m, esEfectivo);
            decimal saldo = totalUsd - montoPrima;
            decimal montoMensualidad = Redondear(saldo / paquete.CantidadMensualidades, esEfectivo);

            return new CotizacionDTO
            {
                PaqueteId = paquete.Id,
                PaqueteNombre = paquete.Nombre,
                CantidadPersonas = cantidadPersonas,
                CantidadNoches = cantidadNoches,
                FormaPago = forma,
                PrecioPersonaNoche = paquete.PrecioPersonaNoche,
                PorcentajePrima = paquete.PorcentajePrima,
                CantidadMensualidades = paquete.CantidadMensualidades,
                Subtotal = subtotal,
                PorcentajeDescuento = porcentajeDescuento,
                MontoDescuento = montoDescuento,
                PorcentajeIva = PorcentajeIva,
                MontoIva = montoIva,
                TotalUsd = totalUsd,
                TipoCambio = tipoCambio,
                TotalColones = totalColones,
                MontoPrima = montoPrima,
                Saldo = saldo,
                MontoMensualidad = montoMensualidad
            };
        }

        /// <summary>
        /// Devuelve la forma de pago con su escritura oficial (Efectivo, Cheque o Tarjeta),
        /// sin importar mayúsculas o espacios. Lanza ArgumentException si no es válida.
        /// </summary>
        public static string NormalizarFormaPago(string? formaPago)
        {
            string? valida = FormasPagoValidas.FirstOrDefault(f =>
                string.Equals(f, formaPago?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (valida == null)
                throw new ArgumentException("Forma de pago inválida. Valores permitidos: Efectivo, Cheque, Tarjeta.");

            return valida;
        }

        // 1-2 noches: 0%, 3-6: 10%, 7-9: 15%, 10-12: 20%, 13 o más: 25%
        private static decimal ObtenerPorcentajeDescuento(int noches)
        {
            return noches switch
            {
                >= 13 => 25m,
                >= 10 => 20m,
                >= 7 => 15m,
                >= 3 => 10m,
                _ => 0m
            };
        }

        // Redondeo comercial: 0.5 o más sube. Sin decimales (efectivo) o a dos decimales.
        private static decimal Redondear(decimal valor, bool sinDecimales)
        {
            return Math.Round(valor, sinDecimales ? 0 : 2, MidpointRounding.AwayFromZero);
        }
    }
}
