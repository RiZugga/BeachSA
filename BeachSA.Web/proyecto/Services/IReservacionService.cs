using proyecto.Dto;

namespace proyecto.Services
{
    /// <summary>
    /// Operaciones de reservaciones. Convención de errores (el controlador los traduce a HTTP):
    /// ArgumentException -> 400, KeyNotFoundException -> 404,
    /// TipoCambioNoDisponibleException -> 503.
    /// </summary>
    public interface IReservacionService
    {
        /// <summary>Calcula el precio de una reservación sin guardar nada.</summary>
        Task<CotizacionDTO> CotizarAsync(CotizacionRequestDTO request, CancellationToken cancellationToken = default);

        /// <summary>Lista las reservaciones activas, las más recientes primero.</summary>
        Task<List<ReservacionResponseDTO>> ListarAsync();

        /// <summary>Busca una reservación activa por su identificador.</summary>
        Task<ReservacionResponseDTO> BuscarPorIdAsync(int id);

        /// <summary>Calcula, guarda (con transacción) y devuelve la reservación nueva.</summary>
        Task<ReservacionResponseDTO> CrearAsync(ReservacionDTO request, int usuarioId, CancellationToken cancellationToken = default);

        /// <summary>Modifica una reservación; recalcula solo si cambia algo que afecta el precio.</summary>
        Task<ReservacionResponseDTO> EditarAsync(int id, ReservacionDTO request, CancellationToken cancellationToken = default);

        /// <summary>Eliminación lógica: marca la reservación como inactiva.</summary>
        Task EliminarAsync(int id);
    }
}
