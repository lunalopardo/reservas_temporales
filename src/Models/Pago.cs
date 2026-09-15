using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una reserva.")]
        [Display(Name = "Reserva")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El concepto no puede superar los 150 caracteres.")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El importe es obligatorio.")]
        [Range(0.01, 9999999999.99, ErrorMessage = "El importe debe ser un valor positivo mayor a 0.")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        public bool Activo { get; set; } = true;

        // Campos auditables (Manejados por sesión)
        public int CreadoPorUserId { get; set; }

        public int? AnuladoPorUserId { get; set; }

        // Propiedades relacionales / adicionales para mostrar en vistas
        public string NombreInquilino { get; set; } = string.Empty;
        public string DireccionInmueble { get; set; } = string.Empty;
    }
}