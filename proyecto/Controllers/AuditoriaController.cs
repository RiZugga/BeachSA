using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = FuncionesSistema.Auditoria)]
    public class AuditoriaController : ControllerBase
    {
        private readonly DbContextBank dbContext;

        public AuditoriaController(DbContextBank pContext)
        {
            this.dbContext = pContext;
        }

        // GET api/Auditoria?desde=2026-11-01&hasta=2026-11-30&accion=Crear&nombreUsuario=admin&pagina=1&tamanoPagina=20
        [HttpGet]
        public async Task<IActionResult> Consultar([FromQuery] AuditoriaFiltroDto filtro)
        {
            var consulta = this.dbContext.RegistrosAuditoria.AsNoTracking().AsQueryable();

            if (filtro.Desde.HasValue)
            {
                var desde = filtro.Desde.Value.Date;
                consulta = consulta.Where(r => r.Fecha >= desde);
            }

            if (filtro.Hasta.HasValue)
            {
                var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
                consulta = consulta.Where(r => r.Fecha < hastaExclusivo);
            }

            if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
            {
                var nombre = filtro.NombreUsuario.Trim();
                consulta = consulta.Where(r => r.NombreUsuario.Contains(nombre));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Accion))
            {
                var accion = filtro.Accion.Trim();
                consulta = consulta.Where(r => r.Accion == accion);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Entidad))
            {
                var entidad = filtro.Entidad.Trim();
                consulta = consulta.Where(r => r.Entidad == entidad);
            }

            int pagina = Math.Max(filtro.Pagina, 1);
            int tamano = Math.Clamp(filtro.TamanoPagina, 1, 100);

            int total = await consulta.CountAsync();

            var items = await consulta
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.Id)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return Ok(new
            {
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamano,
                Items = items
            });
        }
    }
}
