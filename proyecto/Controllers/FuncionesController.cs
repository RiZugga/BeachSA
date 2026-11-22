using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto.Models;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RolesSistema.Administrador)]
    public class FuncionesController : ControllerBase
    {
        // GET api/Funciones
        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(FuncionesSistema.Todas);
        }

        // GET api/Funciones/rol/Regular
        [HttpGet("rol/{rol}")]
        public IActionResult ListarPorRol(string rol)
        {
            if (!FuncionesSistema.RolExiste(rol))
            {
                return NotFound(new { Msj = $"El rol '{rol}' no existe." });
            }

            return Ok(FuncionesSistema.ObtenerFunciones(rol));
        }
    }
}
