using System.ComponentModel.DataAnnotations;

namespace proyecto.Dto
{
    public class PaqueteDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public decimal PrecioPersonaNoche { get; set; }
        
        [Required]
        public decimal PorcentajePrima { get; set; }
        
        [Required]
        public int CantidadMensualidades { get; set; }
    }
}
