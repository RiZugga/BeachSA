using System.ComponentModel.DataAnnotations;

namespace proyecto.Dto
{
    public class ReservacionDTO
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int PaqueteId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadPersonas { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadNoches { get; set; }

        /// <summary>Efectivo, Cheque o Tarjeta.</summary>
        [Required]
        [StringLength(10)]
        public string FormaPago { get; set; } = string.Empty;

        /// <summary>Obligatorio solo cuando la forma de pago es Cheque.</summary>
        [StringLength(30)]
        public string? NumeroCheque { get; set; }

        /// <summary>Obligatorio solo cuando la forma de pago es Cheque.</summary>
        [StringLength(100)]
        public string? Banco { get; set; }

        /// <summary>Indica si la reservación está activa o no.</summary>
        public bool Activo { get; set; } = true;
    }
}
