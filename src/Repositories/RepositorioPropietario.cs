using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioPropietario : RepositorioBase
{
    public RepositorioPropietario(IConfiguration configuration) : base(configuration) { }


    public List<Propietario> GetActivos()
    {
        var listado = new List<Propietario>();
        var query = "SELECT id, nombre, apellido FROM Propietario WHERE activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(new Propietario
            {
                IdPropietario = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido")
            });
        }

        return listado;
    }

    public IList<Propietario> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 5)
    {
        IList<Propietario> res = new List<Propietario>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT id, nombre, apellido, dni, telefono, email, activo 
                FROM Propietario
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
                        res.Add(ParsePropietario(reader));
                    }
                }
            }
        }
        return res;
    }

    // Trae el total de registros activos (aplicando el filtro de búsqueda si existe)
    public int ObtenerCantidad(string? buscar = null)
    {
        int total = 0;
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(Id)
                FROM Propietario
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

    public Propietario? GetById(int id)
    {
        var query = "SELECT * FROM Propietario WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParsePropietario(reader);
        }

        return null;
    }

    // Chequear si existe el correo en la bd
    public bool ExistsEmail(string email, int? idExcluir = null)
    {
        var query = @"SELECT COUNT(*) FROM Propietario 
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

    public int Create(Propietario propietario)
    {
        var query = @"INSERT INTO Propietario (nombre, apellido, dni, email, telefono, activo) 
                      VALUES (@nombre, @apellido, @dni, @email, @telefono, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", propietario.Nombre);
        command.Parameters.AddWithValue("@apellido", propietario.Apellido);
        command.Parameters.AddWithValue("@dni", propietario.Dni);
        command.Parameters.AddWithValue("@email", propietario.Email);
        command.Parameters.AddWithValue("@telefono", (object?)propietario.Telefono ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Update(Propietario propietario)
    {
        var query = @"UPDATE Propietario 
                      SET nombre = @nombre, apellido = @apellido, dni = @dni, email = @email, telefono = @telefono 
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", propietario.IdPropietario);
        command.Parameters.AddWithValue("@nombre", propietario.Nombre);
        command.Parameters.AddWithValue("@apellido", propietario.Apellido);
        command.Parameters.AddWithValue("@dni", propietario.Dni);
        command.Parameters.AddWithValue("@email", propietario.Email);
        command.Parameters.AddWithValue("@telefono", (object?)propietario.Telefono ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteLogico(int id)
    {
        var query = "UPDATE Propietario SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private static Propietario ParsePropietario(MySqlDataReader reader)
    {
        return new Propietario
        {
            IdPropietario = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            Dni = reader.GetString("dni"),
            Email = reader.GetString("email"),
            Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
            Activo = reader.GetBoolean("activo")
        };
    }
}