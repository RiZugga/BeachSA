using examen.Models;
using Microsoft.EntityFrameworkCore;

namespace examen.Dto
{
    public class DbContextBank: DbContext
    {

        public DbContextBank(DbContextOptions<DbContextBank> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Reservacion> Reservaciones { get; set; }

        public DbSet<Paquete> Paquetes { get; set; }
    }
}
