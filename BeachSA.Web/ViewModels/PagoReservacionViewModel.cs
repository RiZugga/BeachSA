using System.ComponentModel.DataAnnotations;
using BeachSA.Web.Models;

namespace BeachSA.Web.ViewModels
{
    
    public class PagoReservacionViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una reservación válida.")]
        [Display(Name = "Reservación")]
        public int ReservacionId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La mensualidad debe ser mayor que cero.")]
        [Display(Name = "Número de mensualidad")]
        public int NumeroMensualidad { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        [Required]
        public string Moneda { get; set; } = string.Empty;

        public Reservacion? Reservacion { get; set; }
    }
}