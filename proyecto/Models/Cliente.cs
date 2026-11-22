using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto.Models
{
    public class Cliente
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Cedula { get; set; }

        public string TipoCedula { get; set; }

        public string NombreCompleto { get; set; }

        public string Telefono {  get; set; }
        
        public string Direccion {  get; set; }

        public string Email { get; set; }

        public bool Activo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaCreacion { get; set; }

    }
}
