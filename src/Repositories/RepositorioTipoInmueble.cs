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

    // Método con paginación y búsqueda para la tabla del Index
    public (List<TipoInmueble> Listado, int TotalPaginas) GetPaginado(string? buscar, int pagina, int tamanoPagina = 10)
    {
        var listado = new List<TipoInmueble>();
        int totalRegistros = 0;

        var whereClause = "WHERE activo = 1";
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            whereClause += " AND nombre LIKE @buscar";
        }

        using MySqlConnection connection = new(connectionString);
        connection.Open();

        // Obtener total de registros
        var countQuery = $"SELECT COUNT(*) FROM TipoInmueble {whereClause}";
        using (MySqlCommand countCommand = new(countQuery, connection))
        {
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                countCommand.Parameters.AddWithValue("@buscar", $"%{buscar}%");
            }
            totalRegistros = Convert.ToInt32(countCommand.ExecuteScalar());
        }

        // Obtener registros paginados
        int offset = (pagina - 1) * tamanoPagina;
        var query = $@"SELECT * FROM TipoInmueble 
                       {whereClause} 
                       ORDER BY id DESC 
                       LIMIT @limit OFFSET @offset";

        using (MySqlCommand command = new(query, connection))
        {
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                command.Parameters.AddWithValue("@buscar", $"%{buscar}%");
            }
            command.Parameters.AddWithValue("@limit", tamanoPagina);
            command.Parameters.AddWithValue("@offset", offset);

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(ParseTipoInmueble(reader));
            }
        }

        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
        return (listado, totalPaginas == 0 ? 1 : totalPaginas);
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