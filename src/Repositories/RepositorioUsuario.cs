using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioUsuario : RepositorioBase
{
    public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

    // Iniciar sesión
    public Usuario? ValidarLogin(string nombreUsuario, string password)
    {
        string sql = @"
            SELECT * FROM usuario 
            WHERE nombre_usuario = @nombreUsuario 
              AND password = @password 
              AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
        command.Parameters.AddWithValue("@password", password);

        connection.Open();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                return ParseUsuario(reader);
            }
        }
        return null;
    }

    // Listado de empleados
    public IList<Usuario> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Usuario> listado = new List<Usuario>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new(connectionString))
        {
            string sql = @"
                SELECT * FROM usuario
                WHERE (@buscar IS NULL OR nombre_usuario LIKE @buscar 
                                       OR nombre LIKE @buscar 
                                       OR apellido LIKE @buscar 
                                       OR email LIKE @buscar)
                ORDER BY id DESC
                LIMIT @limit OFFSET @offset";

            using (MySqlCommand command = new(sql, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                command.Parameters.AddWithValue("@limit", tamPagina);
                command.Parameters.AddWithValue("@offset", offset);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(ParseUsuario(reader));
                    }
                }
            }
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        int total = 0;
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new(connectionString))
        {
            string sql = @"
                SELECT COUNT(id) FROM usuario
                WHERE (@buscar IS NULL OR nombre_usuario LIKE @buscar 
                                       OR nombre LIKE @buscar 
                                       OR apellido LIKE @buscar 
                                       OR email LIKE @buscar)";

            using (MySqlCommand command = new(sql, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    total = Convert.ToInt32(result);
                }
            }
        }
        return total;
    }

    // Obtener pro id
    public Usuario? GetById(int id)
    {
        string sql = "SELECT * FROM usuario WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                return ParseUsuario(reader);
            }
        }
        return null;
    }

    // Crear usuario nuevo (por ahora no está implementado, solo se van a usar usuarios existentes)
    public int Create(Usuario usuario)
    {
        string sql = @"INSERT INTO usuario 
                      (nombre_usuario, nombre, apellido, email, password, avatar, rol, activo) 
                      VALUES 
                      (@nombreUsuario, @nombre, @apellido, @email, @password, @avatar, @rol, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
        command.Parameters.AddWithValue("@rol", usuario.Rol);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Actualizar perfil
    public int UpdatePerfil(Usuario usuario)
    {
        string sql = @"UPDATE usuario SET 
                      nombre = @nombre,
                      apellido = @apellido,
                      email = @email,
                      password = @password,
                      avatar = @avatar
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", usuario.Id);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Actualización solo para admin
    public int UpdateCompleto(Usuario usuario)
    {
        string sql = @"UPDATE usuario SET 
                  nombre = @nombre,
                  apellido = @apellido,
                  email = @email,
                  password = @password,
                  avatar = @avatar,
                  rol = @rol,
                  activo = @activo
                  WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", usuario.Id);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
        command.Parameters.AddWithValue("@rol", usuario.Rol);
        command.Parameters.AddWithValue("@activo", usuario.Activo);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado por si hace falta
    public int Delete(int id)
    {
        string sql = "DELETE FROM usuario WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico (el que está implementado)
    public int AnularLogico(int id)
    {
        string sql = "UPDATE usuario SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Reactivar usuarios (solo admin)
    public int Reactivar(int id)
    {
        string sql = "UPDATE usuario SET Activo = 1 WHERE Id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    internal static Usuario ParseUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt32("id"),
            NombreUsuario = reader.GetString("nombre_usuario"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            Email = reader.GetString("email"),
            Password = reader.GetString("password"),
            Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? null : reader.GetString("avatar"),
            Rol = Convert.ToInt32(reader["rol"]),
            Activo = reader.GetBoolean("activo")
        };
    }
}