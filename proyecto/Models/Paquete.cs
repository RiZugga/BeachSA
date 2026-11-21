using System.ComponentModel.DataAnnotations;

namespace proyecto.Models
{
    public class Paquete
    {

        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public double PrecioPersonaNoche { get; set; }

        public double PorcentajePrima { get; set; }

        public int CantidadMensualidades { get; set; }

        public bool Activo { get; set; }
    }
}