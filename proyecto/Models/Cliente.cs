using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    public class Cliente
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Cedula { get; set; }

        [Required]
        public string TipoCedula { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Telefono {  get; set; }
        
        [Required]
        public string Direccion {  get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool Activo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaCreacion { get; set; }

    }
}
