using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    public class Reservacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Cliente))]
        public int ClienteId { get; set; }

        [Required]
        [ForeignKey(nameof(Paquete))]
        public int PaqueteId { get; set; }

        [Required]
        [ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadPersonas { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CantidadNoches { get; set; }

        [Required]
        [StringLength(10)]
        public string FormaPago { get; set; }

        [StringLength(30)]
        public string? NumeroCheque { get; set; }

        [StringLength(100)]
        public string? Banco { get; set; }

        [Required]
        public decimal PrecioPersonaNoche { get; set; }

        [Required]
        public decimal PorcentajePrima { get; set; }

        [Required]
        public int CantidadMensualidades { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MontoDescuento { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MontoIva { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalUsd { get; set; }

        [Required]
        public decimal PorcentajeDescuento { get; set; }

        [Required]
        public decimal PorcentajeIva { get; set; }

        [Required]
        public decimal TipoCambio { get; set; }

        [Required]
        public decimal TotalColones { get; set; }

        [Required]
        public decimal MontoPrima { get; set; }

        [Required]
        public decimal Saldo { get; set; }

        [Required]
        public decimal MontoMensualidad { get; set; }

        [Required]
        public bool CorreoEnviado { get; set; }

        public DateTime? FechaCorreo { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaCreacion { get; set; }

        public virtual Usuario? Usuario { get; set; }
        public virtual Cliente? Cliente { get; set; }
        public virtual Paquete? Paquete { get; set; }
    }
}