namespace proyecto.Services
{
    public interface IReservacionCorreoService
    {
        Task<byte[]> GenerarPdfAsync(int reservacionId);

        Task EnviarPdfAsync(int reservacionId);

        Task ReenviarPdfAsync(int reservacionId);
    }
}
