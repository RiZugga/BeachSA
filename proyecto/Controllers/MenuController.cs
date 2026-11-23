using Microsoft.AspNetCore.Mvc;
using proyecto.Models;
using Microsoft.AspNetCore.Authorization;

namespace proyecto.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        [HttpGet]
        public IActionResult ObtenerMenu()
        {
            string? rol = FuncionesSistema.ObtenerRol(User);

            return Ok(new
            {
                Rol = rol,
                Funciones = FuncionesSistema.ObtenerFunciones(rol)
            });
        }
    }
}
