using proyecto.Dto;

namespace proyecto.Services
{
    public interface ITipoCambioService
    {
        /// <summary>
        /// Devuelve el tipo de cambio de venta del dólar. Consulta primero a Hacienda y,
        /// si falla, usa el servicio de respaldo. Lanza TipoCambioNoDisponibleException
        /// si ninguno responde.
        /// </summary>
        Task<TipoCambioDTO> ObtenerVentaAsync(CancellationToken cancellationToken = default);
    }
}
