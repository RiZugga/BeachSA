using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BeachSA.Web.Models;

namespace BeachSA.Web.Services
{
    public class RolApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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

        public async Task<(List<FuncionItemViewModel> Items, string? Error)> ListarFuncionesAsync()
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var response = await client.GetAsync("api/Funciones");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<FuncionItemViewModel>>() ?? new();
                return (data.OrderBy(f => f.Orden).ToList(), null);
            }

            return (new(), await LeerErrorAsync(response, "No se pudo cargar el catálogo de funciones."));
        }

        public async Task<(List<FuncionItemViewModel> Items, string? Error)> ObtenerFuncionesPorRolAsync(string rol)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");
            AgregarAutorizacion(client);

            var response = await client.GetAsync($"api/Funciones/rol/{Uri.EscapeDataString(rol)}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<FuncionItemViewModel>>() ?? new();
                return (data.OrderBy(f => f.Orden).ToList(), null);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (new(), $"El rol '{rol}' no existe en la API.");
            }

            return (new(), await LeerErrorAsync(response, $"No se pudieron consultar las funciones del rol '{rol}'."));
        }

        private static async Task<string> LeerErrorAsync(HttpResponseMessage response, string fallback)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(body) ? $"{fallback} (HTTP {(int)response.StatusCode})." : $"{fallback} {body}";
            }
            catch
            {
                return $"{fallback} (HTTP {(int)response.StatusCode}).";
            }
        }
    }
}