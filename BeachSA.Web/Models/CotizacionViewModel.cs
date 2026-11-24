using System.ComponentModel.DataAnnotations;

namespace BeachSA.Web.Models
{
    public class CotizacionViewModel
    {
        public int PaqueteId { get; set; }

        public string PaqueteNombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CantidadPersonas { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int CantidadNoches { get; set; } = 1;

        [Required]
        public string FormaPago { get; set; } = string.Empty;

        public string? NumeroCheque { get; set; }

        public string? Banco { get; set; }

        public Cotizacion Resultado { get; set; } = new Cotizacion();

        public bool CotizacionCalculada { get; set; }
    }
}