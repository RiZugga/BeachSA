using proyecto.Dto;
using proyecto.Models;
using proyecto.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DbContextBank context = null;
        private readonly IAuthorizationServices authorizationServices = null;

        public UsersController(DbContextBank _context, IAuthorizationServices pServices)
        {
            this.context = _context;
            this.authorizationServices = pServices;
        }

        /*
            [HttpPost]
            [Route("AuthenticationUser")]
            public IActionResult AuthenticationUser(User temp)
            {
                var authorization = authorizationServices.DevolverToken(temp);

                if (authorization.Result)
                {
                    return Ok(authorization);
                }
                else
                {
                    return Unauthorized(authorization);
                }
            }

            [HttpPost]
            [Route("RegisterUser")]
            public IActionResult RegisterUser(User temp)
            {
                context.Users.Add(temp);
                context.SaveChanges();
                return Ok(temp);
            }

           */
    }

}
