using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioPago : RepositorioBase
{
    public RepositorioPago(IConfiguration configuration) : base(configuration) { }

    public IList<Pago> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Pago> listado = new List<Pago>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT p.*, 
                       CONCAT(i.nombre, ' ', i.apellido) AS nombre_inquilino,
                       inm.direccion AS direccion_inmueble
                FROM Pago p
                INNER JOIN Reserva r ON p.id_reserva = r.id
                INNER JOIN Inquilino i ON r.id_inquilino = i.id
                INNER JOIN Inmueble inm ON r.id_inmueble = inm.id
                WHERE (@buscar IS NULL OR p.concepto LIKE @buscar 
                                       OR CAST(p.id_reserva AS CHAR) LIKE @buscar
                                       OR CONCAT(i.nombre, ' ', i.apellido) LIKE @buscar
                                       OR inm.direccion LIKE @buscar)
                ORDER BY p.id DESC
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
                        listado.Add(ParsePago(reader));
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
                SELECT COUNT(p.id) 
                FROM Pago p
                INNER JOIN Reserva r ON p.id_reserva = r.id
                INNER JOIN Inquilino i ON r.id_inquilino = i.id
                INNER JOIN Inmueble inm ON r.id_inmueble = inm.id
                WHERE (@buscar IS NULL OR p.concepto LIKE @buscar 
                                       OR CAST(p.id_reserva AS CHAR) LIKE @buscar
                                       OR CONCAT(i.nombre, ' ', i.apellido) LIKE @buscar
                                       OR inm.direccion LIKE @buscar)";

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

    public Pago? GetById(int id)
    {
        string sql = @"
            SELECT p.*, 
                   CONCAT(i.nombre, ' ', i.apellido) AS nombre_inquilino,
                   inm.direccion AS direccion_inmueble
            FROM Pago p
            INNER JOIN Reserva r ON p.id_reserva = r.id
            INNER JOIN Inquilino i ON r.id_inquilino = i.id
            INNER JOIN Inmueble inm ON r.id_inmueble = inm.id
            WHERE p.id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                return ParsePago(reader);
            }
        }

        return null;
    }

    public IList<Pago> GetPorReserva(int idReserva)
    {
        IList<Pago> listado = new List<Pago>();
        string sql = @"
            SELECT p.*, 
                   CONCAT(i.nombre, ' ', i.apellido) AS nombre_inquilino,
                   inm.direccion AS direccion_inmueble
            FROM Pago p
            INNER JOIN Reserva r ON p.id_reserva = r.id
            INNER JOIN Inquilino i ON r.id_inquilino = i.id
            INNER JOIN Inmueble inm ON r.id_inmueble = inm.id
            WHERE p.id_reserva = @idReserva 
            ORDER BY p.fecha_pago DESC";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@idReserva", idReserva);

        connection.Open();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                listado.Add(ParsePago(reader));
            }
        }

        return listado;
    }

    public int Create(Pago pago)
    {
        string sql = @"INSERT INTO Pago 
                      (id_reserva, concepto, fecha_pago, importe, activo, creado_por_user_id) 
                      VALUES 
                      (@idReserva, @concepto, @fechaPago, @importe, 1, @creadoPorUserId)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@idReserva", pago.IdReserva);
        command.Parameters.AddWithValue("@concepto", pago.Concepto);
        command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
        command.Parameters.AddWithValue("@importe", pago.Importe);
        command.Parameters.AddWithValue("@creadoPorUserId", (object?)pago.CreadoPorUserId ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Update(Pago pago)
    {
        string sql = @"UPDATE Pago SET 
                      id_reserva = @idReserva,
                      concepto = @concepto,
                      fecha_pago = @fechaPago,
                      importe = @importe
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", pago.Id);
        command.Parameters.AddWithValue("@idReserva", pago.IdReserva);
        command.Parameters.AddWithValue("@concepto", pago.Concepto);
        command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
        command.Parameters.AddWithValue("@importe", pago.Importe);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int AnularLogico(int id, int? anuladoPorUserId = null)
    {
        string sql = @"UPDATE Pago SET activo = 0, anulado_por_user_id = @anuladoPorUserId WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@anuladoPorUserId", (object?)anuladoPorUserId ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int ReactivarLogico(int id)
    {
        string sql = @"UPDATE Pago SET activo = 1, anulado_por_user_id = NULL WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    internal static Pago ParsePago(MySqlDataReader reader)
    {
        var pago = new Pago
        {
            Id = reader.GetInt32("id"),
            IdReserva = reader.GetInt32("id_reserva"),
            Concepto = reader.GetString("concepto"),
            FechaPago = reader.GetDateTime("fecha_pago"),
            Importe = reader.GetDecimal("importe"),
            Activo = reader.GetBoolean("activo"),
            CreadoPorUserId = reader.IsDBNull(reader.GetOrdinal("creado_por_user_id")) ? 0 : reader.GetInt32("creado_por_user_id"),
            AnuladoPorUserId = reader.IsDBNull(reader.GetOrdinal("anulado_por_user_id")) ? null : reader.GetInt32("anulado_por_user_id")
        };

        // Asignación de columnas resultantes del JOIN
        if (HasColumn(reader, "nombre_inquilino") && !reader.IsDBNull(reader.GetOrdinal("nombre_inquilino")))
        {
            pago.NombreInquilino = reader.GetString("nombre_inquilino");
        }

        if (HasColumn(reader, "direccion_inmueble") && !reader.IsDBNull(reader.GetOrdinal("direccion_inmueble")))
        {
            pago.DireccionInmueble = reader.GetString("direccion_inmueble");
        }

        return pago;
    }

    private static bool HasColumn(MySqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}