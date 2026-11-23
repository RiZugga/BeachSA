using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    [Table("Auditoria")]
    public class RegistroAuditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int? UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(30)]
        [Column(TypeName = "varchar(30)")]
        public string Accion { get; set; } = string.Empty;

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? Entidad { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? EntidadId { get; set; }

        public string? Detalle { get; set; }

        [StringLength(45)]
        [Column(TypeName = "varchar(45)")]
        public string? DireccionIp { get; set; }
    }

    public static class AccionesAuditoria
    {
        public const string Login = "Login";
        public const string LoginFallido = "LoginFallido";
        public const string Crear = "Crear";
        public const string Modificar = "Modificar";
        public const string Eliminar = "Eliminar";
    }
}
