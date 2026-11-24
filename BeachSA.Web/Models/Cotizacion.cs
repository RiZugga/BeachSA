namespace BeachSA.Web.Models
{
    public class Cotizacion
    {
        public int PaqueteId { get; set; }
        public string PaqueteNombre { get; set; } = string.Empty;
        public int CantidadPersonas { get; set; }
        public int CantidadNoches { get; set; }
        public string FormaPago { get; set; } = string.Empty;

        public decimal PrecioPersonaNoche { get; set; }
        public decimal PorcentajePrima { get; set; }
        public int CantidadMensualidades { get; set; }

        public decimal Subtotal { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal PorcentajeIva { get; set; }
        public decimal MontoIva { get; set; }
        public decimal TotalUsd { get; set; }

        public decimal TipoCambio { get; set; }
        public decimal TotalColones { get; set; }

        public decimal MontoPrima { get; set; }
        public decimal Saldo { get; set; }
        public decimal MontoMensualidad { get; set; }
    }
}