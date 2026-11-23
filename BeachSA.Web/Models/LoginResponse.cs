namespace BeachSA.Web.Models
{
    // Modelo para recibir la respuesta de autenticación de la API
    public class LoginResponse
    {
        // Token JWT generado por la API
        public string Token { get; set; } = string.Empty;

        // Fecha y hora de vencimiento del token
        public DateTime result { get; set; }

        // Nombre de usuario
        public string UserName { get; set; } = string.Empty;

        // Nombre completo del usuario
        public string FullName { get; set; } = string.Empty;

        // Roles asignados al usuario
        public List<string> Roles { get; set; } = new();

        // Opciones del menú autorizadas
        public List<MenuItemResponse> Menu { get; set; } = new();
    }

    // Modelo para las opciones del menú
    public class MenuItemResponse
    {
        // Código de la función
        public string Code { get; set; } = string.Empty;

        // Nombre visible en el menú
        public string Title { get; set; } = string.Empty;

        // Ruta de navegación
        public string Route { get; set; } = string.Empty;
    }
}
