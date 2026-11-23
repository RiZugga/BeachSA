using System.ComponentModel.DataAnnotations;

namespace BeachSA.Web.ViewModels
{
    // Modelo para el formulario de inicio de sesión
    public class LoginViewModel
    {
        // Nombre de usuario
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Nombre de usuario")]
        public string Email { get; set; } = string.Empty;

        // Contraseña
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}
