using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
    [Table("TipoInmueble")]
    public class TipoInmueble
    {
        [Key]
        [Column("id")]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de inmueble es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        [Column("nombre")]
        [Display(Name = "Tipo de Inmueble")]
        public string Nombre { get; set; } = string.Empty;

        [Column("activo")]
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}