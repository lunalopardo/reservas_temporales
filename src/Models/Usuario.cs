using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservasTemporales.Models
{
[Table("Usuario")]
public class Usuario
{
[Key]
[Column("id")]
public int Id { get; set; }


    [Required]
    [Column("nombre_usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("avatar")]
    public string? Avatar { get; set; }

    [Required]
    [Column("rol")]
    public string Rol { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;
}


}
