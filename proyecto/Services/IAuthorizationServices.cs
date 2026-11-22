using proyecto.Dto;

namespace proyecto.Services
{
    public interface IAuthorizationServices
    {
        Task<AuthorizationResponse> DevolverTokenAsync(LoginRequest request);
    }
}