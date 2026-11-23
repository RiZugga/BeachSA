namespace proyecto.Dto
{
    /// <summary>
    /// Resultado del cálculo de precios. Todos los montos están en dólares,
    /// excepto TotalColones. Es lo que devuelve el endpoint de cotización y lo
    /// que se guarda en la reservación.
    /// </summary>
    public class CotizacionDTO
    {
        public int PaqueteId { get; set; }
        public string PaqueteNombre { get; set; } = string.Empty;
        public int CantidadPersonas { get; set; }
        public int CantidadNoches { get; set; }
        public string FormaPago { get; set; } = string.Empty;

        // Valores del paquete usados en el cálculo (se congelan en la reservación)
        public decimal PrecioPersonaNoche { get; set; }
        public decimal PorcentajePrima { get; set; }
        public int CantidadMensualidades { get; set; }

        public decimal Subtotal { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal PorcentajeIva { get; set; }
        public decimal MontoIva { get; set; }
        public decimal TotalUsd { get; set; }

        /// <summary>Tipo de cambio de venta usado para convertir a colones.</summary>
        public decimal TipoCambio { get; set; }
        public decimal TotalColones { get; set; }

        public decimal MontoPrima { get; set; }
        public decimal Saldo { get; set; }
        public decimal MontoMensualidad { get; set; }
    }
}
