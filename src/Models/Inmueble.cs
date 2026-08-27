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

        [Display(Name = "Inmueble Disponible")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Inmueble Activo")]
        public bool Activo { get; set; } = true;
    }
}