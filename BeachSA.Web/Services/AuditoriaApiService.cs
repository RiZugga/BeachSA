using System.Net.Http.Headers;
using System.Text.Json;
using BeachSA.Web.Dtos;
using Microsoft.AspNetCore.WebUtilities;

namespace BeachSA.Web.Services
{
    public interface IAuditoriaApiService
    {
        Task<AuditoriaResultadoDto?> ObtenerAsync(AuditoriaFiltroDto filtro);
    }

    public class AuditoriaApiService : IAuditoriaApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuditoriaApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuditoriaResultadoDto?> ObtenerAsync(AuditoriaFiltroDto filtro)
        {
            var client = _httpClientFactory.CreateClient("BeachApi");

            var query = new Dictionary<string, string?>();

            if (filtro.Desde.HasValue)
                query["desde"] = filtro.Desde.Value.ToString("yyyy-MM-dd");

            if (filtro.Hasta.HasValue)
                query["hasta"] = filtro.Hasta.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
                query["nombreUsuario"] = filtro.NombreUsuario.Trim();

            if (!string.IsNullOrWhiteSpace(filtro.Accion))
                query["accion"] = filtro.Accion.Trim();

            if (!string.IsNullOrWhiteSpace(filtro.Entidad))
                query["entidad"] = filtro.Entidad.Trim();

            query["pagina"] = Math.Max(filtro.Pagina, 1).ToString();
            query["tamanoPagina"] = Math.Clamp(filtro.TamanoPagina, 1, 100).ToString();

            string url = QueryHelpers.AddQueryString("api/Auditoria", query);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Lee el token real de la sesión iniciada (Claim o Session)
            var httpContext = _httpContextAccessor.HttpContext;
            var token = httpContext?.User.FindFirst("AccessToken")?.Value
                        ?? httpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AuditoriaResultadoDto>(stream, _jsonOptions);
        }
    }
}