using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioInquilino : RepositorioBase
{
    public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

    // Traemos la lista paginada 
    public IList<Inquilino> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 5)
    {
        IList<Inquilino> res = new List<Inquilino>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT * FROM Inquilino 
                WHERE activo = 1 
                  AND (@buscar IS NULL OR nombre LIKE @buscar OR apellido LIKE @buscar OR dni LIKE @buscar)
                ORDER BY id
                LIMIT @limit OFFSET @offset";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                command.Parameters.AddWithValue("@limit", tamPagina);
                command.Parameters.AddWithValue("@offset", offset);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ParseInquilino(reader));
                    }
                }
            }
        }
        return res;
    }

    // Traemos el total de registros activos filtrados
    public int ObtenerCantidad(string? buscar = null)
    {
        int total = 0;
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(id) 
                FROM Inquilino 
                WHERE activo = 1 
                  AND (@buscar IS NULL OR nombre LIKE @buscar OR apellido LIKE @buscar OR dni LIKE @buscar)";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
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

    //Obtener Inquilino por ID
    public Inquilino? GetById(int id)
    {
        var query = "SELECT * FROM Inquilino WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseInquilino(reader);
        }

        return null;
    }

    // Chequear si ya existe el correo en la BD (devuelve true si ya está en uso)
    public bool ExistsEmail(string email, int? idExcluir = null)
    {
        var query = @"SELECT COUNT(*) FROM Inquilino 
                      WHERE email = @email 
                      AND (@idExcluir IS NULL OR id != @idExcluir)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@idExcluir", (object?)idExcluir ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Chequear si existe el dni en la bd
    public bool ExistsDNI(string dni, int? idExcluir = null)
    {
        var query = @"SELECT COUNT(*) FROM Inquilino 
                      WHERE dni = @dni 
                      AND (@idExcluir IS NULL OR id != @idExcluir)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@dni", dni);
        command.Parameters.AddWithValue("@idExcluir", (object?)idExcluir ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // CREAR Inquilino
    public int Create(Inquilino inquilino)
    {
        var query = @"INSERT INTO Inquilino (nombre, apellido, dni, email, telefono, activo) 
                      VALUES (@nombre, @apellido, @dni, @email, @telefono, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
        command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
        command.Parameters.AddWithValue("@dni", inquilino.Dni);
        command.Parameters.AddWithValue("@email", inquilino.Email);
        command.Parameters.AddWithValue("@telefono", (object?)inquilino.Telefono ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Editar Inquilino
    public int Update(Inquilino inquilino)
    {
        var query = @"UPDATE Inquilino 
                      SET nombre = @nombre, apellido = @apellido, dni = @dni, email = @email, telefono = @telefono 
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", inquilino.IdInquilino);
        command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
        command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
        command.Parameters.AddWithValue("@dni", inquilino.Dni);
        command.Parameters.AddWithValue("@email", inquilino.Email);
        command.Parameters.AddWithValue("@telefono", (object?)inquilino.Telefono ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico
    public int DeleteLogico(int id)
    {
        var query = "UPDATE Inquilino SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public IList<Inquilino> GetParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Inquilino>();

        string sql = @"
        SELECT id, nombre, apellido, dni 
        FROM Inquilino 
        WHERE activo = 1";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            sql += " AND (nombre LIKE @buscar OR apellido LIKE @buscar OR dni LIKE @buscar)";
        }

        sql += " ORDER BY apellido, nombre ASC LIMIT @limite";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    command.Parameters.AddWithValue("@buscar", $"%{buscar.Trim()}%");
                }
                command.Parameters.AddWithValue("@limite", limite);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(new Inquilino
                        {
                            IdInquilino = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Dni = reader.GetString("dni")
                        });
                    }
                }
            }
        }

        return listado;
    }

    /*función auxiliar privada encargada de transformar una fila devuelta por MySQL en un objeto C# de tipo Inquilino (para reutilizar)*/
    private static Inquilino ParseInquilino(MySqlDataReader reader)
    {
        return new Inquilino
        {
            IdInquilino = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            Dni = reader.GetString("dni"),
            Email = reader.GetString("email"),
            Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
            Activo = reader.GetBoolean("activo")
        };
    }
}