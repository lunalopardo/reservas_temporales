using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioInquilino : RepositorioBase
{
    public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

    public (List<Inquilino> Listado, int TotalPaginas) GetPaginado(string? buscar, int pagina, int registrosPorPagina)
    {
        var listado = new List<Inquilino>();
        int totalRegistros = 0;

        using MySqlConnection connection = new(connectionString);
        connection.Open();

        var countQuery = @"SELECT COUNT(*) FROM Inquilino 
                           WHERE activo = 1 
                           AND (@buscar IS NULL OR nombre LIKE @buscar OR apellido LIKE @buscar OR dni LIKE @buscar)";

        using (MySqlCommand countCommand = new(countQuery, connection))
        {
            var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";
            countCommand.Parameters.AddWithValue("@buscar", paramBuscar);
            totalRegistros = Convert.ToInt32(countCommand.ExecuteScalar());
        }

        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);
        int offset = (pagina - 1) * registrosPorPagina;

        /*Aplica el mismo filtro de búsqueda y estado activo, pero agrega LIMIT (cuántos traer) y OFFSET (cuántos ignorar al principio) 
        para traer solo la porción de datos correspondiente a la página solicitada.*/
        var query = @"SELECT * FROM Inquilino 
                      WHERE activo = 1 
                      AND (@buscar IS NULL OR nombre LIKE @buscar OR apellido LIKE @buscar OR dni LIKE @buscar)
                      LIMIT @limit OFFSET @offset";

        using (MySqlCommand command = new(query, connection))
        {
            var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";
            command.Parameters.AddWithValue("@buscar", paramBuscar);
            command.Parameters.AddWithValue("@limit", registrosPorPagina);
            command.Parameters.AddWithValue("@offset", Math.Max(0, offset));

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(ParseInquilino(reader));
            }
        }

        return (listado, totalPaginas);
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