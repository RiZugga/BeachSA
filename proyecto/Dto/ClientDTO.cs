using System.ComponentModel.DataAnnotations;

namespace proyecto.Dto
{
    public class CrearClienteDTO
    {
        [Required]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public string TipoCedula { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ClientDTO : CrearClienteDTO
    {
        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
