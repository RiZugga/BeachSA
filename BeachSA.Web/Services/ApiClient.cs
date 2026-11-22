using System.Net.Http.Json;
using BeachSA.Web.Models;
using BeachSA.Web.ViewModels;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BeachSA.Web.Services
{
    // Servicio encargado de la comunicación con la API
    public class ApiClient
    {

        /* Configuración del Cliente HTTP */

        // Cliente HTTP utilizado para realizar solicitudes
        private readonly HttpClient _httpClient;

        // Acceso al contexto de la solicitud HTTP actual
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor del servicio
        public ApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }


        /* AUTORIZACIÓN MEDIANTE JWT */

        // Obtener el token del usuario autenticado
        private string? ObtenerToken()
        {
            // Obtener el usuario de la solicitud actual
            var usuario = _httpContextAccessor.HttpContext?.User;

            // Verificar que exista una sesión autenticada
            if (usuario?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Recuperar el JWT guardado durante el Login
            return usuario.FindFirst("AccessToken")?.Value;
        }

        // Crear una solicitud HTTP con autorización
        private HttpRequestMessage CrearSolicitudAutorizada(
            HttpMethod metodo,
            string ruta)
        {
            // Preparar la solicitud
            var solicitud = new HttpRequestMessage(metodo, ruta);

            // Obtener el token de la sesión
            var token = ObtenerToken();

            // Agregar autorización si existe un JWT
            if (!string.IsNullOrWhiteSpace(token))
            {
                solicitud.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return solicitud;
        }


        /* AUTENTICACIÓN */

        // Enviar credenciales a la API para iniciar sesión
        public async Task<LoginResponse?> LoginAsync(LoginViewModel model)
        {
            // Enviar solicitud POST con usuario y contraseña
            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                model
            );

            // Si la API rechaza la solicitud, no hay autenticación válida
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Convertir la respuesta JSON a nuestro modelo
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }


        /* CONSULTA DE CLIENTES */

        // Obtener todos los clientes desde la API
        public async Task<List<ClienteViewModel>?> ObtenerClientesAsync()
        {
            // Crear una solicitud GET con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Get,
                "api/clientes"
            );

            // Enviar la solicitud a la API
            using var respuesta = await _httpClient.SendAsync(solicitud);

            // Verificar que la respuesta sea correcta
            respuesta.EnsureSuccessStatusCode();

            // Convertir la respuesta JSON en una lista de clientes
            return await respuesta.Content
                .ReadFromJsonAsync<List<ClienteViewModel>>();
        }


        /* CONSULTA DE CLIENTE POR IDENTIFICADOR */

        // Obtener la información de un cliente específico
        public async Task<ClienteViewModel?> ObtenerClientePorIdAsync(int id)
        {
            // Validar que el identificador sea positivo
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "El identificador debe ser mayor que cero."
                );
            }

            // Crear solicitud GET con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Get,
                $"api/clientes/{id}"
            );

            // Enviar solicitud a la API
            using var respuesta = await _httpClient.SendAsync(solicitud);

            // Si el cliente no existe, devolver null
            if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            // Verificar que la respuesta sea correcta
            respuesta.EnsureSuccessStatusCode();

            // Convertir el JSON recibido en un cliente
            return await respuesta.Content
                .ReadFromJsonAsync<ClienteViewModel>();
        }


        /* REGISTRO DE CLIENTES */

        // Enviar los datos de un cliente nuevo a la API
        public async Task<HttpResponseMessage> CrearClienteAsync(
            ClienteViewModel cliente)
        {
            // Crear una solicitud POST con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Post,
                "api/clientes"
            );

            // Convertir los datos del cliente en JSON
            solicitud.Content = JsonContent.Create(cliente);

            // Enviar la solicitud a la API
            return await _httpClient.SendAsync(solicitud);
        }


        /* ACTUALIZACIÓN DE CLIENTES */

        // Enviar los datos modificados de un cliente a la API
        public async Task<HttpResponseMessage> ActualizarClienteAsync(
            int id,
            ClienteViewModel cliente)
        {
            // Validar el identificador del cliente
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            // Crear una solicitud PUT con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Put,
                $"api/clientes/{id}"
            );

            // Convertir los datos modificados a JSON
            solicitud.Content = JsonContent.Create(cliente);

            // Enviar la solicitud a la API
            return await _httpClient.SendAsync(solicitud);
        }


        /* ELIMINACIÓN DE CLIENTES */

        // Solicitar la eliminación o desactivación de un cliente
        public async Task<HttpResponseMessage> EliminarClienteAsync(int id)
        {
            // Validar el identificador del cliente
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            // Crear una solicitud DELETE con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Delete,
                $"api/clientes/{id}"
            );

            // Enviar la solicitud a la API
            return await _httpClient.SendAsync(solicitud);
        }


        /* BÚSQUEDA DE CLIENTES POR NOMBRE */

        // Buscar clientes utilizando su nombre
        public async Task<List<ClienteViewModel>?> BuscarClientesPorNombreAsync(
            string nombre)
        {
            // Validar que el nombre no esté vacío
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return new List<ClienteViewModel>();
            }

            // Preparar el texto para utilizarlo en la URL
            string nombreCodificado = Uri.EscapeDataString(nombre.Trim());

            // Crear solicitud GET con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Get,
                $"api/clientes/searchdescrip/{nombreCodificado}"
            );

            // Enviar la solicitud a la API
            using var respuesta = await _httpClient.SendAsync(solicitud);

            // Verificar el resultado
            respuesta.EnsureSuccessStatusCode();

            // Convertir el JSON en una lista de clientes
            return await respuesta.Content
                .ReadFromJsonAsync<List<ClienteViewModel>>();
        }


        /* CONSULTA DE INFORMACIÓN POR CÉDULA */

        // Consultar información de una persona mediante su cédula
        public async Task<HttpResponseMessage> ConsultarCedulaAsync(
            string cedula)
        {
            // Validar que la cédula no esté vacía
            if (string.IsNullOrWhiteSpace(cedula))
            {
                throw new ArgumentException(
                    "Debe indicar una cédula.",
                    nameof(cedula)
                );
            }

            // Preparar la cédula para utilizarla en la URL
            string cedulaCodificada = Uri.EscapeDataString(cedula.Trim());

            // Crear solicitud GET con autenticación
            using var solicitud = CrearSolicitudAutorizada(
                HttpMethod.Get,
                $"api/clientes/consulta-cedula/{cedulaCodificada}"
            );

            // Enviar la solicitud a la API
            return await _httpClient.SendAsync(solicitud);
        }



    }
}
