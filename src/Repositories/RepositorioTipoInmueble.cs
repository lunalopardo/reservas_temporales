using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioTipoInmueble : RepositorioBase
{
    public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration) { }

    public List<TipoInmueble> GetAll()
    {
        var listado = new List<TipoInmueble>();
        var query = "SELECT id, nombre FROM TipoInmueble";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(new TipoInmueble
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre")
            });
        }

        return listado;
    }

    // Método para llenar selects
    public List<TipoInmueble> GetAllActivos()
    {
        var listado = new List<TipoInmueble>();
        var query = "SELECT * FROM TipoInmueble WHERE activo = 1 ORDER BY nombre";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseTipoInmueble(reader));
        }

        return listado;
    }

    // Métodos con paginación y búsqueda para la tabla del Index
    public IList<TipoInmueble> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<TipoInmueble> listado = new List<TipoInmueble>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
            SELECT * 
            FROM TipoInmueble
            WHERE activo = 1 
              AND (@buscar IS NULL OR nombre LIKE @buscar)
            ORDER BY id DESC
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
                        listado.Add(ParseTipoInmueble(reader));
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

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
            SELECT COUNT(id) 
            FROM TipoInmueble
            WHERE activo = 1 
              AND (@buscar IS NULL OR nombre LIKE @buscar)";

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
    public TipoInmueble? GetById(int id)
    {
        var query = "SELECT * FROM TipoInmueble WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseTipoInmueble(reader);
        }

        return null;
    }

    public int Create(TipoInmueble tipo)
    {
        var query = "INSERT INTO TipoInmueble (nombre, activo) VALUES (@nombre, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", tipo.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Update(TipoInmueble tipo)
    {
        var query = "UPDATE TipoInmueble SET nombre = @nombre WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", tipo.Id);
        command.Parameters.AddWithValue("@nombre", tipo.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteLogico(int id)
    {
        var query = "UPDATE TipoInmueble SET activo = 0 WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private static TipoInmueble ParseTipoInmueble(MySqlDataReader reader)
    {
        return new TipoInmueble
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Activo = reader.GetBoolean("activo")
        };
    }
}