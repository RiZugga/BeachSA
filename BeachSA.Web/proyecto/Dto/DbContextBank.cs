using proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace proyecto.Dto
{
    public class DbContextBank: DbContext
    {

        public DbContextBank(DbContextOptions<DbContextBank> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Reservacion> Reservaciones { get; set; }

        public DbSet<Paquete> Paquetes { get; set; }
    }
}
