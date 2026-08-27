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

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El campo {0} solo puede contener letras.")]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El campo {0} solo puede contener letras.")]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "El campo {0} debe tener entre {2} y {1} dígitos.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El campo {0} solo puede contener números.")]
        [Column("dni")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Teléfono")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} dígitos.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El campo {0} solo puede contener números.")]
        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El correo debe tener un usuario válido (mínimo 3 caracteres antes del @) y un dominio válido.")]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}