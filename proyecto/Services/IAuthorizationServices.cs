using examen.Dto;
using examen.Models;

namespace examen.Services
{
    public interface IAuthorizationServices
    {
        AuthorizationResponse DevolverToken(User authorization);
    }
}
