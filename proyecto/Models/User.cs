using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace examen.Models
{
    public class User
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Cedula { get; set; }

        public string TipoCedula { get; set; }

        public string NombreCompleto { get; set; }

        public string Teléfono {  get; set; }
        
        public string Dirección {  get; set; }

        public bool Activo { get; set; }

        public string Rol { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaCreación { get; set; }

    }
}
