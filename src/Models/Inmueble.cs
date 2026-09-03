using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
    public enum TipoInmueble
    {
        Casa,
        Departamento,
        Cabaña,
        Habitacion,
        Otro
    }

    public class Inmueble
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El propietario es obligatorio.")]
        [Display(Name = "Propietario")]
        [ForeignKey("Propietario")]
        public int IdPropietario { get; set; }

        public virtual Propietario? Propietario { get; set; }

        [Required(ErrorMessage = "Seleccione el tipo de inmueble.")]
        [Display(Name = "Tipo de Inmueble")]
        public TipoInmueble Tipo { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cupo máximo es obligatorio.")]
        [Range(1, 50, ErrorMessage = "El cupo debe ser de al menos 1 persona y no superar las 50.")]
        [Display(Name = "Cupo de Personas")]
        public int Cupo { get; set; }

        [RegularExpression(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|(\d{1,2}))(\.\d+)?)$",
            ErrorMessage = "Las coordenadas deben tener un formato válido (ej. -34.6037, -58.3816).")]
        [Display(Name = "Coordenadas GPS")]
        public string? Coord { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 10000000.00, ErrorMessage = "El precio debe ser un valor positivo válido.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio por día")]
        public decimal Precio { get; set; }

        // Mapeo a LONGTEXT para soportar Data URLs de Base64
        [Column(TypeName = "longtext")]
        [Display(Name = "Foto de Portada")]
        public string? FotoPortada { get; set; }

        // Strings concatenados con '|' tipo LONGTEXT
        [Column(TypeName = "longtext")]
        [Display(Name = "Galería de Fotos")]
        public string? Fotos { get; set; }

        [Display(Name = "Inmueble Activo")]
        public bool Activo { get; set; } = true;

        // Propiedad de navegación hacia sus Reservas
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        // Propiedad calculada para ver el estado del inmueble HOY (y actualizar la tabla de inm según reservas)
        [NotMapped]
        [Display(Name = "Disponible Hoy")]
        public bool EstaDisponibleHoy
        {
            get
            {
                // Si está dado de baja manualmente, no está disponible
                if (!Activo) return false;

                var hoy = DateTime.Today;

                // Verificamos si existe alguna reserva activa que ocupe la fecha de hoy
                bool estaOcupadoHoy = Reservas != null && Reservas.Any(r =>
                    r.Activo &&
                    r.FechaDesde.Date <= hoy &&
                    r.FechaHasta.Date >= hoy
                );

                return !estaOcupadoHoy;
            }
        }
    }
}