using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
    [Table("Reserva")]
    public class Reserva
    {
        // ============================================================
        // ID
        // ============================================================

        [Key]
        [Column("id")]
        [Display(Name = "Codigo")]
        public int Id { get; set; }


        // ============================================================
        // INMUEBLE
        // ============================================================

        [Required(ErrorMessage = "Debe seleccionar un inmueble.")]
        [Display(Name = "Inmueble")]
        [Column("id_inmueble")]
        public int IdInmueble { get; set; }

        [ForeignKey(nameof(IdInmueble))]
        public virtual Inmueble? Inmueble { get; set; }


        // ============================================================
        // INQUILINO
        // ============================================================

        [Required(ErrorMessage = "Debe seleccionar un inquilino.")]
        [Display(Name = "Inquilino")]
        [Column("id_inquilino")]
        public int IdInquilino { get; set; }

        [ForeignKey(nameof(IdInquilino))]
        public virtual Inquilino? Inquilino { get; set; }


        // ============================================================
        // FECHAS
        // ============================================================

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Fecha de inicio")]
        [Column("fecha_desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; }


        [Required(ErrorMessage = "La fecha de finalizacion es obligatoria.")]
        [Display(Name = "Fecha de finalización")]
        [Column("fecha_hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; }


        // ============================================================
        // MONTO
        // ============================================================

        [Required(ErrorMessage = "El monto diario es obligatorio.")]
        [Display(Name = "Monto diario")]
        [Column("monto_diario", TypeName = "decimal(12,2)")]
        public decimal MontoDiario { get; set; }


        // ============================================================
        // USUARIO QUE CREO LA RESERVA
        // ============================================================

        [Required]
        [Display(Name = "Creado por")]
        [Column("creado_por_user_id")]
        public int CreadoPorUserId { get; set; }

        [ForeignKey(nameof(CreadoPorUserId))]
        public virtual Usuario? CreadoPorUsuario { get; set; }


        // ============================================================
        // USUARIO QUE TERMINO LA RESERVA
        // ============================================================

        [Display(Name = "Terminado por")]
        [Column("terminado_por_user_id")]
        public int? TerminadoPorUserId { get; set; }

        [ForeignKey(nameof(TerminadoPorUserId))]
        public virtual Usuario? TerminadoPorUsuario { get; set; }


        // ============================================================
        // ESTADO
        // ============================================================

        [Display(Name = "Activa")]
        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}