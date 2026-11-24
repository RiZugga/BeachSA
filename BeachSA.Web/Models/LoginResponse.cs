namespace BeachSA.Web.Models
{
    // Modelo para recibir la respuesta de autenticación de la API
    public class LoginResponse
    {
        // Token JWT generado por la API
        public string Token { get; set; } = string.Empty;

        // Resultado de la autenticación (true si fue exitosa, false en caso contrario)
        public bool result { get; set; }

        // Mensaje de la autenticación (puede contener errores o información adicional)
        public string msj { get; set; } = string.Empty;

    }

    // Modelo para las opciones del menú
    public class MenuItemResponse
    {
        // Código de la función
        public string Codigo { get; set; } = string.Empty;

        // Nombre visible en el menú
        public string Nombre { get; set; } = string.Empty;

        // Orden en el menú
        public int Orden { get; set; } = 0;
    }
}
