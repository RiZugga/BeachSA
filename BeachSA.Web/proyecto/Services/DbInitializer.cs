using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyecto.Dto;
using proyecto.Models;

namespace proyecto.Services
{
    public static class DbInitializer
    {
        public static async Task CrearAdministradorInicialAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DbContextBank>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            try
            {
                if (await context.Usuarios.AnyAsync(u => u.Rol == RolesSistema.Administrador))
                {
                    return;
                }

                var seccion = config.GetSection("AdminInicial");
                string? email = seccion["Email"];
                string? password = seccion["Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("No se creó el administrador inicial: falta la sección AdminInicial en appsettings.json.");
                    return;
                }

                var admin = new Usuario
                {
                    NombreUsuario = seccion["NombreUsuario"] ?? "admin",
                    NombreCompleto = seccion["NombreCompleto"] ?? "Administrador del Sistema",
                    Email = email,
                    Rol = RolesSistema.Administrador,
                    Activo = true
                };
                admin.PasswordHash = hasher.HashPassword(admin, password);

                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
                logger.LogInformation("Administrador inicial creado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER:");
                    Console.WriteLine(ex.InnerException.ToString());
                }

                throw;
            }
        }
    }
}