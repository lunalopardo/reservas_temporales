using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ReservasTemporales.Models;

public enum enRoles
{
    Administrador = 1,
    Empleado = 2
}

[Table("usuario")]
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

    [Required, EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("avatar")]
    public string? Avatar { get; set; }

    [NotMapped]
    public IFormFile? AvatarFile { get; set; }

    [Required]
    [Column("rol")]
    public int Rol { get; set; } = (int)enRoles.Empleado;

    [NotMapped]
    public string RolNombre => Rol == 1 ? "Administrador" : "Empleado";

    [Column("activo")]
    public bool Activo { get; set; } = true;

    public static IDictionary<int, string> ObtenerRoles()
    {
        var roles = new SortedDictionary<int, string>();
        foreach (var valor in Enum.GetValues(typeof(enRoles)))
        {
            roles.Add((int)valor, (string)Enum.GetName(typeof(enRoles), valor)!);
        }
        return roles;
    }
}