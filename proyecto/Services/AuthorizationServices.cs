using proyecto.Dto;
using proyecto.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace proyecto.Services
{
    public class AuthorizationServices : IAuthorizationServices
    {
        private readonly DbContextBank dbContext = null;
        private readonly IConfiguration configuration = null;

        public AuthorizationServices(IConfiguration pConfig, DbContextBank pContext)
        {
            this.dbContext = pContext;
            this.configuration = pConfig;
        }

        public AuthorizationResponse DevolverToken(User authorization)
        {
            User temp = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(authorization.Email) && x.PasswordHash.Equals(authorization.PasswordHash));

            string token = "";

            if (temp != null)
            {
                token = this.GenerarToken(temp.Email);
                return new AuthorizationResponse() { Token = token, Result = true, Msj = "OK" };

            }else
            {
                return new AuthorizationResponse() { Token = "", Result = false, Msj = "Auth failed..." };
            }
        }

        private string GenerarToken(string email)
        {
            string tokenCreado = "";
            var Key = this.configuration.GetValue<string>("JWTSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(Key);
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, email));
            var credencialesToken = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfi = tokenHandler.CreateToken(tokenDescriptor);

            tokenCreado = tokenHandler.WriteToken(tokenConfi);
            return tokenCreado;
        }
    }
}
