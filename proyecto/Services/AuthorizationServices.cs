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
        private readonly IAuditoriaServices auditoria;

        public AuthorizationServices(IConfiguration pConfig, DbContextBank pContext, IPasswordHasher<Usuario> pHasher, IAuditoriaServices pAuditoria)
        {
            this.dbContext = pContext;
            this.configuration = pConfig;
            this.passwordHasher = pHasher;
            this.auditoria = pAuditoria;
        }

        public async Task<AuthorizationResponse> DevolverTokenAsync(LoginRequest request)
        {
            string email = request.Email.Trim();

            Usuario? usuario = await this.dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

            // La respuesta al cliente es siempre la misma; en la auditoría sí se anota el motivo real
            if (usuario == null)
            {
                await RegistrarLoginFallidoAsync(null, email, $"Correo '{email}' no registrado.");
                return Fallo();
            }

            if (!usuario.Activo)
            {
                await RegistrarLoginFallidoAsync(usuario, email, "Usuario inactivo.");
                return Fallo();
            }

            var resultado = this.passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                await RegistrarLoginFallidoAsync(usuario, email, "Contraseña incorrecta.");
                return Fallo();
            }

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = this.passwordHasher.HashPassword(usuario, request.Password);
                await this.dbContext.SaveChangesAsync();
            }

            await this.auditoria.RegistrarAsync(
                AccionesAuditoria.Login, "Auth", usuario.Id.ToString(), "Inicio de sesión exitoso.",
                usuarioId: usuario.Id, nombreUsuario: usuario.NombreUsuario);

            return new AuthorizationResponse() { Token = this.GenerarToken(usuario), Result = true, Msj = "OK" };
        }

        private Task RegistrarLoginFallidoAsync(Usuario? usuario, string emailIntentado, string motivo)
        {
            return this.auditoria.RegistrarAsync(
                AccionesAuditoria.LoginFallido, "Auth", usuario?.Id.ToString(), motivo,
                usuarioId: usuario?.Id,
                nombreUsuario: usuario?.NombreUsuario ?? emailIntentado);
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