using System.ComponentModel.DataAnnotations;

namespace proyecto.Dto
{
    /// <summary>
    /// Datos mínimos para cotizar una reservación (no requiere cliente ni cheque).
    /// </summary>
    public class CotizacionRequestDTO
    {
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
    }
}
