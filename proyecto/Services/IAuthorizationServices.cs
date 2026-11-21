using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    public interface IAuthorizationServices
    {
        AuthorizationResponse DevolverToken(User authorization);
    }
}
