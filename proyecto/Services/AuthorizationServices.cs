using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using proyecto.Dto;
using proyecto.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace proyecto.Services
{
    public class AuthorizationServices : IAuthorizationServices
    {
        private const int MinutosVencimientoToken = 8;   // requerimiento del proyecto
        private const string MensajeCredencialesInvalidas = "Credenciales inválidas.";

        private readonly DbContextBank dbContext;
        private readonly IConfiguration configuration;
        private readonly IPasswordHasher<Usuario> passwordHasher;

        public AuthorizationServices(IConfiguration pConfig, DbContextBank pContext, IPasswordHasher<Usuario> pHasher)
        {
            this.dbContext = pContext;
            this.configuration = pConfig;
            this.passwordHasher = pHasher;
        }

        public async Task<AuthorizationResponse> DevolverTokenAsync(LoginRequest request)
        {
            string email = request.Email.Trim();

            Usuario? usuario = await this.dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

            // Mismo mensaje para correo inexistente, usuario inactivo o contraseña incorrecta
            if (usuario == null || !usuario.Activo)
            {
                return Fallo();
            }

            var resultado = this.passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Fallo();
            }

            // Si el algoritmo del hash se actualizó, se guarda el hash nuevo
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = this.passwordHasher.HashPassword(usuario, request.Password);
                await this.dbContext.SaveChangesAsync();
            }

            return new AuthorizationResponse() { Token = this.GenerarToken(usuario), Result = true, Msj = "OK" };
        }

        private static AuthorizationResponse Fallo()
        {
            return new AuthorizationResponse() { Token = "", Result = false, Msj = MensajeCredencialesInvalidas };
        }

        private string GenerarToken(Usuario usuario)
        {
            string key = this.configuration.GetValue<string>("JwtSettings:Key")
                ?? throw new InvalidOperationException("Falta JwtSettings:Key en appsettings.json");
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Name, usuario.NombreUsuario));
            claims.AddClaim(new Claim(ClaimTypes.Email, usuario.Email));

            if (!string.IsNullOrWhiteSpace(usuario.Rol))
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, usuario.Rol));
            }

            var credencialesToken = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(MinutosVencimientoToken),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfi = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(tokenConfi);
        }
    }
}