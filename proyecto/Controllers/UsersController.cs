using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;
using System.Security.Claims;

namespace proyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = FuncionesSistema.Usuarios)]
    public class UsersController : ControllerBase
    {
        private readonly DbContextBank dbContext;
        private readonly IPasswordHasher<Usuario> passwordHasher;

        public UsersController(DbContextBank pContext, IPasswordHasher<Usuario> pHasher)
        {
            this.dbContext = pContext;
            this.passwordHasher = pHasher;
        }

        // GET api/Users
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await this.dbContext.Usuarios.AsNoTracking()
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return Ok(usuarios.Select(Mapear));
        }

        // GET api/Users/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Buscar(int id)
        {
            var usuario = await this.dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new { Msj = "Usuario no encontrado." });
            }

            return Ok(Mapear(usuario));
        }

        // GET api/Users/buscar?valor=ana
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPorDescripcion([FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return BadRequest(new { Msj = "Debe indicar un valor de búsqueda." });
            }

            valor = valor.Trim();

            var usuarios = await this.dbContext.Usuarios.AsNoTracking()
                .Where(u => u.NombreUsuario.Contains(valor)
                         || u.NombreCompleto.Contains(valor)
                         || u.Email.Contains(valor))
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return Ok(usuarios.Select(Mapear));
        }

        // POST api/Users
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] UserCreateDto dto)
        {
            string? rol = FuncionesSistema.NormalizarRol(dto.Rol);
            if (rol == null)
            {
                return RolInvalido(dto.Rol);
            }

            string email = dto.Email.Trim();
            string nombreUsuario = dto.NombreUsuario.Trim();

            string? duplicado = await ValidarDuplicadosAsync(email, nombreUsuario);
            if (duplicado != null)
            {
                return Conflict(new { Msj = duplicado });
            }

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                NombreCompleto = dto.NombreCompleto.Trim(),
                Email = email,
                Rol = rol,
                Activo = dto.Activo
            };
            usuario.PasswordHash = this.passwordHasher.HashPassword(usuario, dto.Password);

            this.dbContext.Usuarios.Add(usuario);
            await this.dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Buscar), new { id = usuario.Id }, Mapear(usuario));
        }

        // PUT api/Users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] UserUpdateDto dto)
        {
            var usuario = await this.dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new { Msj = "Usuario no encontrado." });
            }

            if (id == ObtenerIdUsuarioActual() && !dto.Activo)
            {
                return BadRequest(new { Msj = "No puede desactivar su propio usuario." });
            }

            string email = dto.Email.Trim();
            string nombreUsuario = dto.NombreUsuario.Trim();

            string? duplicado = await ValidarDuplicadosAsync(email, nombreUsuario, id);
            if (duplicado != null)
            {
                return Conflict(new { Msj = duplicado });
            }

            usuario.NombreUsuario = nombreUsuario;
            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.Email = email;
            usuario.Activo = dto.Activo;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                usuario.PasswordHash = this.passwordHasher.HashPassword(usuario, dto.Password);
            }

            await this.dbContext.SaveChangesAsync();

            return Ok(Mapear(usuario));
        }

        // PUT api/Users/5/rol
        [HttpPut("{id:int}/rol")]
        public async Task<IActionResult> AsignarRol(int id, [FromBody] AsignarRolDto dto)
        {
            string? rol = FuncionesSistema.NormalizarRol(dto.Rol);
            if (rol == null)
            {
                return RolInvalido(dto.Rol);
            }

            if (id == ObtenerIdUsuarioActual())
            {
                return BadRequest(new { Msj = "No puede cambiar su propio rol." });
            }

            var usuario = await this.dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new { Msj = "Usuario no encontrado." });
            }

            usuario.Rol = rol;
            await this.dbContext.SaveChangesAsync();

            return Ok(Mapear(usuario));
        }

        // DELETE api/Users/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id == ObtenerIdUsuarioActual())
            {
                return BadRequest(new { Msj = "No puede eliminar su propio usuario." });
            }

            var usuario = await this.dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(new { Msj = "Usuario no encontrado." });
            }

            try
            {
                this.dbContext.Usuarios.Remove(usuario);
                await this.dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { Msj = "No se puede eliminar el usuario porque tiene registros asociados. Desactívelo en su lugar." });
            }

            return NoContent();
        }

        // ---------- Métodos auxiliares ----------

        private int? ObtenerIdUsuarioActual()
        {
            string? valor = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : null;
        }

        private async Task<string?> ValidarDuplicadosAsync(string email, string nombreUsuario, int idExcluido = 0)
        {
            if (await this.dbContext.Usuarios.AnyAsync(u => u.Email == email && u.Id != idExcluido))
            {
                return "Ya existe un usuario con ese correo.";
            }

            if (await this.dbContext.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != idExcluido))
            {
                return "Ya existe un usuario con ese nombre de usuario.";
            }

            return null;
        }

        private IActionResult RolInvalido(string? rol)
        {
            return BadRequest(new { Msj = $"El rol '{rol}' no es válido. Roles permitidos: {RolesSistema.Administrador}, {RolesSistema.Regular}." });
        }

        private static UserResponseDto Mapear(Usuario u)
        {
            return new UserResponseDto
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion
            };
        }
    }
}