namespace BeachSA.Web.Models
{
    public class Reservacion
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PaqueteId { get; set; }
        public int UsuarioId { get; set; }

        public int CantidadPersonas { get; set; }
        public int CantidadNoches { get; set; }

        public string FormaPago { get; set; }
        public string? NumeroCheque { get; set; }
        public string? Banco { get; set; }

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

        public bool CorreoEnviado { get; set; }
        public DateTime? FechaCorreo { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool Activo { get; set; }
    }
}
