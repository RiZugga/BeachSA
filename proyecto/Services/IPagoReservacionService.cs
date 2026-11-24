using proyecto.Dto;

namespace proyecto.Services
{
    public interface IPagoReservacionService
    {
        Task<PagoResponseDTO> AplicarPagoAsync(
            PagoRequestDTO request,
            int usuarioId,
            CancellationToken cancellationToken = default
        );

        Task<List<PagoResponseDTO>> ObtenerHistorialAsync(
            int reservacionId,
            CancellationToken cancellationToken = default
        );
    }
}