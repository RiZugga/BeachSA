using BeachSA.Web.Dtos;
using BeachSA.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachSA.Web.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaApiService _auditoriaApiService;

        public AuditoriaController(IAuditoriaApiService auditoriaApiService)
        {
            _auditoriaApiService = auditoriaApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AuditoriaFiltroDto filtro)
        {
            filtro.Pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
            filtro.TamanoPagina = filtro.TamanoPagina < 1 ? 20 : filtro.TamanoPagina;

            var resultado = await _auditoriaApiService.ObtenerAsync(filtro)
                ?? new AuditoriaResultadoDto
                {
                    Pagina = filtro.Pagina,
                    TamanoPagina = filtro.TamanoPagina
                };

            ViewBag.Filtro = filtro;
            return View(resultado);
        }
    }
}



