using System.ComponentModel.DataAnnotations;

namespace BeachSA.Web.ViewModels
{
    public class UsuarioItemViewModel
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class UsuarioIndexViewModel
    {
        public string? Filtro { get; set; }
        public List<UsuarioItemViewModel> Usuarios { get; set; } = new();
        public List<string> RolesDisponibles { get; set; } = new() { "Administrador", "Regular" };
    }

    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Regular";

        [Display(Name = "Usuario activo")]
        public bool Activo { get; set; } = true;

        public string Cedula { get; set; } = string.Empty;

        // Tipo de identificación
        [Required(ErrorMessage = "El tipo de cédula es obligatorio.")]
        [Display(Name = "Tipo de cédula")]
        public string TipoCedula { get; set; } = string.Empty;

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

        public List<string> RolesDisponibles { get; set; } = new() { "Administrador", "Regular" };
    }

    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña (opcional)")]
        public string? Password { get; set; }

        [Display(Name = "Usuario activo")]
        public bool Activo { get; set; }
    }

    public class AsignarRolViewModel
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol asignado")]
        public string Rol { get; set; } = string.Empty;

        public List<string> RolesDisponibles { get; set; } = new() { "Administrador", "Regular" };
    }
}