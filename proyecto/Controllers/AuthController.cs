using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto.Dto;
using proyecto.Services;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthorizationServices authorizationServices;

        public AuthController(IAuthorizationServices pServices)
        {
            this.authorizationServices = pServices;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var respuesta = await this.authorizationServices.DevolverTokenAsync(request);

            if (!respuesta.Result)
            {
                return Unauthorized(respuesta);
            }

            return Ok(respuesta);
        }
    }
}
