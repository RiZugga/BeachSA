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
            var token = httpContext?.User?.FindFirst("AccessToken")?.Value
                        ?? httpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = null;
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

            var cedula = Uri.EscapeDataString(model.Cedula?.Trim() ?? "");
            var cedulaValidation = await client.GetAsync($"api/clientes/existscedula?value={cedula}");

            if (!cedulaValidation.IsSuccessStatusCode)
            {
                return (false, await LeerMensajeErrorAsync(cedulaValidation, "No se pudo verificar la cédula"));
            }

            var exists = await cedulaValidation.Content.ReadFromJsonAsync<bool>();
            if (exists)
            {
                return (false, "La cédula ya está registrada.");
            }

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
            if (!response.IsSuccessStatusCode)
            {
                return (false, await LeerMensajeErrorAsync(response, "No se pudo registrar el usuario"));
            }

            // Crear datos del cliente si el usuario fue creado exitosamente
            var payloadClient = new
            {
                nombreCompleto = model.NombreCompleto.Trim(),
                email = model.Email.Trim(),
                tipoCedula = model.TipoCedula?.Trim(),
                cedula = model.Cedula?.Trim(),
                telefono = model.Telefono?.Trim(),
                direccion = model.Direccion?.Trim(),
                activo = model.Activo
            };

            var responseClient = await client.PostAsJsonAsync("api/clientes", payloadClient);

            if (!responseClient.IsSuccessStatusCode)
            {
                return (false, await LeerMensajeErrorAsync(responseClient, "Error al registrar los datos del cliente"));
            }

            return (true, null);
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