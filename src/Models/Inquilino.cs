using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
    [Table("Inquilino")]
    public class Inquilino
    {
        [Key]
        [Column("id")]
        [Display(Name = "Código")]
        public int IdInquilino { get; set; }

        [Required]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [Column("dni")]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}