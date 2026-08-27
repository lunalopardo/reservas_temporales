using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Models;

namespace ReservasTemporales.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }


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
            modelBuilder.Entity<Inquilino>().HasData(
               new Inquilino { IdInquilino = 1, Nombre = "Federico", Apellido = "Morales", Dni = "38123456", Email = "fede.morales@gmail.com", Telefono = "1199887766", Activo = true },
               new Inquilino { IdInquilino = 2, Nombre = "Gabriela", Apellido = "Sosa", Dni = "40987654", Email = "gaby.sosa@live.com", Telefono = "1188776655", Activo = true },
               new Inquilino { IdInquilino = 3, Nombre = "Lucía", Apellido = "Sosa", Dni = "45975677", Email = "lucia.sosa@gmail.com", Telefono = "1198784675", Activo = true }
           );

            // Carga inicial de Inmuebles
            modelBuilder.Entity<Inmueble>().HasData(
                new Inmueble
                {
                    Id = 1,
                    IdPropietario = 1,
                    Tipo = TipoInmueble.Departamento,
                    Direccion = "Av. Corrientes 1234, CABA",
                    Cupo = 4,
                    Coord = "-34.6037,-58.3816",
                    Precio = 45000.00m,
                    FotoPortada = null,
                    Fotos = null,
                    Estado = true,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 2,
                    IdPropietario = 2,
                    Tipo = TipoInmueble.Casa,
                    Direccion = "Calle 50 #432, La Plata",
                    Cupo = 6,
                    Coord = "-34.9214,-57.9545",
                    Precio = 75000.00m,
                    FotoPortada = null,
                    Fotos = null,
                    Estado = true,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 3,
                    IdPropietario = 3,
                    Tipo = TipoInmueble.Cabaña,
                    Direccion = "Ruta 40 Km 12, Bariloche",
                    Cupo = 5,
                    Coord = "-41.1335,-71.3103",
                    Precio = 120000.00m,
                    FotoPortada = null,
                    Fotos = null,
                    Estado = true,
                    Activo = true
                }
            );
        }
    }
}