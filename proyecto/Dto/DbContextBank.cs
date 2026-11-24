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

        public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; }

        public DbSet<PagoReservacion> PagosReservacion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reservacion>()
        .ToTable(tb => tb.HasTrigger("TR_Reservaciones_ResetCorreo"));
        }
    }
}
