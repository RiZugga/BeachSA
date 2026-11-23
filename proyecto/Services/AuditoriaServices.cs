using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;
using System.Security.Claims;

namespace proyecto.Services
{
    public class AuditoriaServices : IAuditoriaServices
    {
        private readonly DbContextBank dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<AuditoriaServices> logger;

        public AuditoriaServices(DbContextBank pContext, IHttpContextAccessor pAccessor, ILogger<AuditoriaServices> pLogger)
        {
            this.dbContext = pContext;
            this.httpContextAccessor = pAccessor;
            this.logger = pLogger;
        }

        public async Task RegistrarAsync(
            string accion,
            string? entidad = null,
            string? entidadId = null,
            string? detalle = null,
            int? usuarioId = null,
            string? nombreUsuario = null)
        {
            RegistroAuditoria? registro = null;

            try
            {
                var httpContext = this.httpContextAccessor.HttpContext;
                var principal = httpContext?.User;
                bool autenticado = principal?.Identity?.IsAuthenticated == true;

                if (usuarioId == null && autenticado)
                {
                    string? valor = principal!.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(valor, out int id))
                    {
                        usuarioId = id;
                    }
                }

                if (nombreUsuario == null && autenticado)
                {
                    nombreUsuario = principal!.FindFirstValue(ClaimTypes.Name);
                }

                registro = new RegistroAuditoria
                {
                    UsuarioId = usuarioId,
                    NombreUsuario = Recortar(nombreUsuario ?? "Anónimo", 50)!,
                    Fecha = DateTime.UtcNow,
                    Accion = Recortar(accion, 30)!,
                    Entidad = Recortar(entidad, 50),
                    EntidadId = Recortar(entidadId, 50),
                    Detalle = detalle,
                    DireccionIp = Recortar(httpContext?.Connection.RemoteIpAddress?.ToString(), 45)
                };

                this.dbContext.RegistrosAuditoria.Add(registro);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (registro != null)
                {
                    this.dbContext.Entry(registro).State = EntityState.Detached;
                }

                this.logger.LogError(ex, "No se pudo registrar la auditoría de la acción {Accion}.", accion);
            }
        }

        private static string? Recortar(string? texto, int maximo)
        {
            if (texto == null)
            {
                return null;
            }

            return texto.Length > maximo ? texto.Substring(0, maximo) : texto;
        }
    }
}
