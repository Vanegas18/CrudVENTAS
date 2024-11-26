using Microsoft.EntityFrameworkCore;
using TallerCrudVentas.Models;

namespace TallerCrudVentas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Propiedad que representa la tabla de ventas en la base de datos
        public DbSet<Models.Producto> Ventas { get; set; }

        // Propiedad que representa la tabla de clientes en la base de datos
        public DbSet<Models.Cliente> Clientes { get; set; }

        // Propiedad que representa la tabla de productos en la base de datos
        public DbSet<Models.Producto> Productos { get; set; }

        public DbSet<Models.DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la entidad Cliente
            modelBuilder.Entity<DetalleVenta>()
                .Property(dv => dv.SubTotal)
                .HasColumnType("decimal(18, 2)");

        }
        public DbSet<TallerCrudVentas.Models.Venta> Venta { get; set; } = default!;
    }
}
