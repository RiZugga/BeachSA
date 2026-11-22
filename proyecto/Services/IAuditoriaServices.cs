namespace proyecto.Services
{
    public interface IAuditoriaServices
    {
        // Si no se indican usuarioId o nombreUsuario, se toman del token de la petición actual
        Task RegistrarAsync(
            string accion,
            string? entidad = null,
            string? entidadId = null,
            string? detalle = null,
            int? usuarioId = null,
            string? nombreUsuario = null);
    }
}
