using System.ComponentModel.DataAnnotations;

namespace BeachSA.Web.ViewModels
{
    // Modelo para los formularios de clientes
    public class ClienteViewModel
    {
        /* Identificación del Cliente */

        // Identificador del cliente
        public int Id { get; set; }

        // Número de cédula
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cédula no puede superar los 20 caracteres.")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        // Tipo de identificación
        [Required(ErrorMessage = "El tipo de cédula es obligatorio.")]
        [Display(Name = "Tipo de cédula")]
        public string TipoCedula { get; set; } = string.Empty;

        /* Información Personal */

        // Nombre completo del cliente
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        // Número de teléfono
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        // Dirección del cliente
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        // Correo electrónico
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        /* Estado del Cliente */

        // Indica si el cliente se encuentra activo
        public bool Activo { get; set; } = true;
    }
}
