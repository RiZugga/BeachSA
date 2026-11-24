using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace proyecto.Models
{
    [Index(nameof(ReservacionId), nameof(NumeroMensualidad), IsUnique = true)]
    public class PagoReservacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservacionId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int NumeroMensualidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // USD o CRC
        [Required]
        [StringLength(3)]
        public string Moneda { get; set; } = string.Empty;

        // Tipo de cambio utilizado al registrar el pago
        [Column(TypeName = "decimal(18,4)")]
        public decimal TipoCambioAplicado { get; set; }

        // Equivalencia del pago en dólares para actualizar el saldo
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoUsd { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        public Reservacion? Reservacion { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
