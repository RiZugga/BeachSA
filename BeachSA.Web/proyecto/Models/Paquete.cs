using System.ComponentModel.DataAnnotations;

namespace proyecto.Models
{
    public class Paquete
    {

        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public decimal PrecioPersonaNoche { get; set; }

        public decimal PorcentajePrima { get; set; }

        public int CantidadMensualidades { get; set; }

        public bool Activo { get; set; }
    }
}