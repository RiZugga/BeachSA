using System.Globalization;
using System.Text.Json;
using proyecto.Dto;

namespace proyecto.Services
{
    /// <summary>
    /// Obtiene el tipo de cambio de venta del dólar. Orden de consulta:
    /// 1) API del Ministerio de Hacienda, 2) API de GoMeta (respaldo).
    /// El resultado se guarda en memoria unos minutos para no consultar en cada cotización.
    /// </summary>
    public class TipoCambioService : ITipoCambioService
    {
        private const string UrlHacienda = "https://api.hacienda.go.cr/indicadores/tc";
        private const string UrlGoMeta = "https://apis.gometa.org/tdc/";

        private static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(30);
        private static readonly object Candado = new();
        private static TipoCambioDTO? cache;
        private static DateTime cacheVenceUtc = DateTime.MinValue;

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<TipoCambioService> logger;

        public TipoCambioService(IHttpClientFactory pHttpClientFactory, ILogger<TipoCambioService> pLogger)
        {
            this.httpClientFactory = pHttpClientFactory;
            this.logger = pLogger;
        }

        public async Task<TipoCambioDTO> ObtenerVentaAsync(CancellationToken cancellationToken = default)
        {
            lock (Candado)
            {
                if (cache != null && DateTime.UtcNow < cacheVenceUtc)
                {
                    return cache;
                }
            }

            TipoCambioDTO? resultado =
                await this.IntentarAsync("Hacienda", UrlHacienda, LeerHacienda, cancellationToken)
                ?? await this.IntentarAsync("GoMeta", UrlGoMeta, LeerGoMeta, cancellationToken);

            if (resultado == null)
            {
                throw new TipoCambioNoDisponibleException();
            }

            lock (Candado)
            {
                cache = resultado;
                cacheVenceUtc = DateTime.UtcNow.Add(DuracionCache);
            }

            return resultado;
        }

        /// <summary>
        /// Consulta una fuente. Devuelve null si falla por cualquier motivo
        /// (red, tiempo de espera, formato inesperado) para poder probar la siguiente.
        /// </summary>
        private async Task<TipoCambioDTO?> IntentarAsync(string fuente, string url,
            Func<JsonElement, TipoCambioDTO> lector, CancellationToken cancellationToken)
        {
            try
            {
                using var cliente = this.httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(8);

                using var respuesta = await cliente.GetAsync(url, cancellationToken);
                respuesta.EnsureSuccessStatusCode();

                string contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
                using var json = JsonDocument.Parse(contenido);

                TipoCambioDTO tipoCambio = lector(json.RootElement);
                tipoCambio.Fuente = fuente;

                if (tipoCambio.Venta <= 0)
                {
                    throw new InvalidOperationException("El tipo de cambio recibido no es válido.");
                }

                return tipoCambio;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                this.logger.LogWarning(ex, "No se pudo obtener el tipo de cambio desde {Fuente}.", fuente);
                return null;
            }
        }

        // Formato de Hacienda: { "dolar": { "venta": { "fecha": "...", "valor": 450.31 }, "compra": {...} }, ... }
        private static TipoCambioDTO LeerHacienda(JsonElement raiz)
        {
            JsonElement venta = raiz.GetProperty("dolar").GetProperty("venta");

            return new TipoCambioDTO
            {
                Venta = LeerDecimal(venta.GetProperty("valor")),
                Fecha = LeerFecha(venta.GetProperty("fecha"))
            };
        }

        // Formato de GoMeta: { "venta": 512.64, "venta_date": "...", "compra": ..., ... }
        private static TipoCambioDTO LeerGoMeta(JsonElement raiz)
        {
            return new TipoCambioDTO
            {
                Venta = LeerDecimal(raiz.GetProperty("venta")),
                Fecha = raiz.TryGetProperty("venta_date", out JsonElement fecha) ? LeerFecha(fecha) : DateTime.Today
            };
        }

        // Acepta el valor como número JSON o como texto
        private static decimal LeerDecimal(JsonElement elemento)
        {
            if (elemento.ValueKind == JsonValueKind.Number)
            {
                return elemento.GetDecimal();
            }

            return decimal.Parse(elemento.GetString() ?? string.Empty, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        // Las fuentes devuelven fechas en formatos distintos; si no se puede leer, se usa hoy
        private static DateTime LeerFecha(JsonElement elemento)
        {
            if (elemento.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(elemento.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                return fecha;
            }

            return DateTime.Today;
        }
    }
}
