using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Models;

namespace ReservasTemporales.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Propietario> Propietarios { get; set; }
        //public DbSet<Inquilino> Inquilinos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Carga inicial de Propietarios
            modelBuilder.Entity<Propietario>().HasData(
                new Propietario { IdPropietario = 1, Nombre = "Alberto", Apellido = "Fernández", Dni = "25111222", Email = "alberto.f@gmail.com", Telefono = "1144556677", Activo = true },
                new Propietario { IdPropietario = 2, Nombre = "Beatriz", Apellido = "López", Dni = "28333444", Email = "beatriz_lopez@hotmail.com", Telefono = "1133221100", Activo = true },
                new Propietario { IdPropietario = 3, Nombre = "Claudio", Apellido = "García", Dni = "31555666", Email = "cgarcia@yahoo.com", Telefono = "1166778899", Activo = true }
            );

            // Carga inicial de Inquilinos
            /* modelBuilder.Entity<Inquilino>().HasData(
                new Inquilino { Id = 1, Nombre = "Federico", Apellido = "Morales", Dni = "38123456", Email = "fede.morales@gmail.com", Telefono = "1199887766", Activo = true },
                new Inquilino { Id = 2, Nombre = "Gabriela", Apellido = "Sosa", Dni = "40987654", Email = "gaby.sosa@live.com", Telefono = "1188776655", Activo = true }
            );*/
        }
    }
}