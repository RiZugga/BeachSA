
using System.ComponentModel.DataAnnotations;

namespace proyecto.Dto
{
    public class PagoRequestDTO
    {
        [Required]
        public int ReservacionId { get; set; }

        [Required]
        public int NumeroMensualidad { get; set; }

        [Required]
        public decimal Monto { get; set; }

        [Required]
        public string Moneda { get; set; } = string.Empty;
    }
}
