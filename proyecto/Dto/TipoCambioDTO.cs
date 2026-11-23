namespace proyecto.Dto
{
    /// <summary>
    /// Tipo de cambio de venta (colones por dólar) obtenido de un servicio externo.
    /// </summary>
    public class TipoCambioDTO
    {
        public decimal Venta { get; set; }

        /// <summary>Fecha de publicación del tipo de cambio según la fuente.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Servicio del que se obtuvo (Hacienda o GoMeta).</summary>
        public string Fuente { get; set; } = string.Empty;
    }
}
