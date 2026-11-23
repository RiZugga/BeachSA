using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BeachSA.Web.Models.Dtos;
using BeachSA.Web.ViewModels;

namespace BeachSA.Web.Services
{
    public interface IUsuarioApiService
    {
        Task<(bool Ok, List<UserResponseDto>? Data, string? Error)> ListarAsync(string? filtro = null);
        Task<(bool Ok, UserResponseDto? Data, string? Error)> ObtenerPorIdAsync(int id);
        Task<(bool Ok, string? Error)> CrearAsync(UsuarioCreateViewModel model);
        Task<(bool Ok, string? Error)> EditarAsync(UsuarioEditViewModel model);
        Task<(bool Ok, string? Error)> AsignarRolAsync(AsignarRolViewModel model);
        Task<(bool Ok, string? Error)> EliminarAsync(int id);
    }

    public class UsuarioApiService : IUsuarioApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsuarioApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AgregarAutorizacion(HttpClient client)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var token = httpContext?.User?.FindFirst("AccessToken")?.Value;

            
            if (string.IsNullOrWhiteSpace(token))
            {
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsImVtYWlsIjoiYWRtaW5AYmVhY2hzYS5jb20iLCJyb2xlIjoiQWRtaW5pc3RyYWRvciIsIm5iZiI6MTc5NTM4NzI3MiwiZXhwIjoxNzk1Mzg3NzUyLCJpYXQiOjE3OTUzODcyNzJ9.EizcXWaXXeT97hH2kkqbHnB6pkco_oc_RtdcesV5sc8";
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<string?> LeerMensajeErrorAsync(HttpResponseMessage response, string fallback)
        {
            try
            {
                var errorJson = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                if (errorJson.TryGetProperty("msg", out var msgProp))
                {
                    var msg = msgProp.GetString();
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        return msg;
                    }
                }
            }
            catch
            {
                // Si el cuerpo no es JSON con "msg", usamos el mensaje por defecto
            }

            return $"{fallback} (HTTP {(int)response.StatusCode}).";
        }

        public async Task<(bool Ok, List<UserResponseDto>? Data, string? Error)> ListarAsync(string? filtro = null)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            string url = string.IsNullOrWhiteSpace(filtro)
                ? "api/Users"
                : $"api/Users/buscar?valor={Uri.EscapeDataString(filtro.Trim())}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, await LeerMensajeErrorAsync(response, "Error al consultar la API"));
            }

            var data = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>(_jsonOptions) ?? new();
            return (true, data, null);
        }

        public async Task<(bool Ok, UserResponseDto? Data, string? Error)> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var response = await client.GetAsync($"api/Users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, await LeerMensajeErrorAsync(response, "Error al consultar el usuario"));
            }

            var data = await response.Content.ReadFromJsonAsync<UserResponseDto>(_jsonOptions);
            return (true, data, null);
        }

        public async Task<(bool Ok, string? Error)> CrearAsync(UsuarioCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var payload = new
            {
                nombreUsuario = model.NombreUsuario.Trim(),
                nombreCompleto = model.NombreCompleto.Trim(),
                email = model.Email.Trim(),
                password = model.Password,
                rol = model.Rol?.Trim(),
                activo = model.Activo
            };

            var response = await client.PostAsJsonAsync("api/Users", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, await LeerMensajeErrorAsync(response, "No se pudo registrar el usuario"));
        }

        public async Task<(bool Ok, string? Error)> EditarAsync(UsuarioEditViewModel model)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var payload = new
            {
                nombreUsuario = model.NombreUsuario.Trim(),
                nombreCompleto = model.NombreCompleto.Trim(),
                password = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password,
                email = model.Email.Trim(),
                activo = model.Activo
            };

            var response = await client.PutAsJsonAsync($"api/Users/{model.Id}", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, await LeerMensajeErrorAsync(response, "No se pudo actualizar el usuario"));
        }

        public async Task<(bool Ok, string? Error)> AsignarRolAsync(AsignarRolViewModel model)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var payload = new
            {
                rol = model.Rol?.Trim()
            };

            var response = await client.PutAsJsonAsync($"api/Users/{model.Id}/rol", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, await LeerMensajeErrorAsync(response, "No se pudo asignar el rol"));
        }

        public async Task<(bool Ok, string? Error)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var response = await client.DeleteAsync($"api/Users/{id}");
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, await LeerMensajeErrorAsync(response, "No se pudo eliminar el usuario"));
        }
    }
}