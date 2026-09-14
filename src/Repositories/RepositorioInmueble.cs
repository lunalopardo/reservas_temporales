using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioInmueble : RepositorioBase
{
    public RepositorioInmueble(IConfiguration configuration) : base(configuration) { }

    public (List<Inmueble> Listado, int TotalPaginas) GetPaginado(string? buscar, int pagina, int tamanoPagina = 10)
    {
        var listado = new List<Inmueble>();
        int totalRegistros = 0;

        var whereClause = "WHERE i.activo = 1";
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            whereClause += @" AND (i.direccion LIKE @buscar 
                               OR p.nombre LIKE @buscar 
                               OR p.apellido LIKE @buscar 
                               OR t.nombre LIKE @buscar)";
        }

        using MySqlConnection connection = new(connectionString);
        connection.Open();

        // Obtener total de registros
        var countQuery = $@"SELECT COUNT(*) 
                            FROM Inmueble i
                            INNER JOIN Propietario p ON i.id_propietario = p.id
                            INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                            {whereClause}";

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
        var query = $@"SELECT i.*, 
                              p.nombre AS PropietarioNombre, p.apellido AS PropietarioApellido,
                              t.nombre AS TipoNombre
                       FROM Inmueble i
                       INNER JOIN Propietario p ON i.id_propietario = p.id
                       INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                       {whereClause}
                       ORDER BY i.id DESC
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
                listado.Add(ParseInmueble(reader));
            }
        }

        // Cargar las reservas de los inmuebles obtenidos para que funcione EstaDisponibleHoy
        if (listado.Any())
        {
            CargarReservasParaInmuebles(connection, listado);
        }

        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
        return (listado, totalPaginas == 0 ? 1 : totalPaginas);
    }

    public Inmueble? GetById(int id)
    {
        var query = @"SELECT i.*, 
                             p.nombre AS PropietarioNombre, p.apellido AS PropietarioApellido,
                             t.nombre AS TipoNombre
                      FROM Inmueble i
                      INNER JOIN Propietario p ON i.id_propietario = p.id
                      INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                      WHERE i.id = @id AND i.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        Inmueble? inmueble = null;
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                inmueble = ParseInmueble(reader);
            }
        }

        if (inmueble != null)
        {
            CargarReservasParaInmuebles(connection, new List<Inmueble> { inmueble });
        }

        return inmueble;
    }

    public List<Inmueble> GetPorPropietario(int idPropietario)
    {
        var listado = new List<Inmueble>();
        var query = @"SELECT i.*, 
                             p.nombre AS PropietarioNombre, p.apellido AS PropietarioApellido,
                             t.nombre AS TipoNombre
                      FROM Inmueble i
                      INNER JOIN Propietario p ON i.id_propietario = p.id
                      INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                      WHERE i.id_propietario = @idPropietario AND i.activo = 1
                      ORDER BY i.id DESC";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idPropietario", idPropietario);

        connection.Open();
        using (MySqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                listado.Add(ParseInmueble(reader));
            }
        }

        if (listado.Any())
        {
            CargarReservasParaInmuebles(connection, listado);
        }

        return listado;
    }

    public int Create(Inmueble inmueble)
    {
        var query = @"INSERT INTO Inmueble 
                      (id_propietario, id_tipo_inmueble, direccion, cupo, coord, precio, foto_portada, fotos, activo)
                      VALUES 
                      (@idPropietario, @idTipoInmueble, @direccion, @cupo, @coord, @precio, @fotoPortada, @fotos, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@idPropietario", inmueble.IdPropietario);
        command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
        command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
        command.Parameters.AddWithValue("@cupo", inmueble.Cupo);
        command.Parameters.AddWithValue("@coord", (object?)inmueble.Coord ?? DBNull.Value);
        command.Parameters.AddWithValue("@precio", inmueble.Precio);
        command.Parameters.AddWithValue("@fotoPortada", (object?)inmueble.Foto_portada ?? DBNull.Value);
        command.Parameters.AddWithValue("@fotos", (object?)inmueble.Fotos ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Update(Inmueble inmueble)
    {
        var query = @"UPDATE Inmueble SET 
                      id_propietario = @idPropietario,
                      id_tipo_inmueble = @idTipoInmueble,
                      direccion = @direccion,
                      cupo = @cupo,
                      coord = @coord,
                      precio = @precio,
                      foto_portada = @fotoPortada,
                      fotos = @fotos
                      WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@id", inmueble.Id);
        command.Parameters.AddWithValue("@idPropietario", inmueble.IdPropietario);
        command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
        command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
        command.Parameters.AddWithValue("@cupo", inmueble.Cupo);
        command.Parameters.AddWithValue("@coord", (object?)inmueble.Coord ?? DBNull.Value);
        command.Parameters.AddWithValue("@precio", inmueble.Precio);
        command.Parameters.AddWithValue("@fotoPortada", (object?)inmueble.Foto_portada ?? DBNull.Value);
        command.Parameters.AddWithValue("@fotos", (object?)inmueble.Fotos ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int DeleteLogico(int id)
    {
        var query = "UPDATE Inmueble SET activo = 0 WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private void CargarReservasParaInmuebles(MySqlConnection connection, List<Inmueble> inmuebles)
    {
        var ids = inmuebles.Select(i => i.Id).ToList();
        var idsParam = string.Join(",", ids);

        // Consultamos las reservas asociadas a estos inmuebles
        var query = $@"SELECT id, id_inmueble, fecha_desde, fecha_hasta, activo 
                       FROM Reserva 
                       WHERE id_inmueble IN ({idsParam})";

        using MySqlCommand command = new(query, connection);
        using MySqlDataReader reader = command.ExecuteReader();
        
        var reservasDict = new Dictionary<int, List<Reserva>>();

        while (reader.Read())
        {
            var reserva = new Reserva
            {
                Id = reader.GetInt32("id"),
                IdInmueble = reader.GetInt32("id_inmueble"),
                FechaDesde = reader.GetDateTime("fecha_desde"),
                FechaHasta = reader.GetDateTime("fecha_hasta"),
                Activo = reader.GetBoolean("activo")
            };

            if (!reservasDict.ContainsKey(reserva.IdInmueble))
            {
                reservasDict[reserva.IdInmueble] = new List<Reserva>();
            }
            reservasDict[reserva.IdInmueble].Add(reserva);
        }

        // Asignamos las reservas a cada inmueble correspondiente
        foreach (var inmueble in inmuebles)
        {
            if (reservasDict.ContainsKey(inmueble.Id))
            {
                inmueble.Reservas = reservasDict[inmueble.Id];
            }
            else
            {
                inmueble.Reservas = new List<Reserva>();
            }
        }
    }

    private static Inmueble ParseInmueble(MySqlDataReader reader)
    {
        return new Inmueble
        {
            Id = reader.GetInt32("id"),
            IdPropietario = reader.GetInt32("id_propietario"),
            Propietario = new Propietario
            {
                IdPropietario = reader.GetInt32("id_propietario"),
                Nombre = reader.GetString("PropietarioNombre"),
                Apellido = reader.GetString("PropietarioApellido")
            },
            IdTipoInmueble = reader.GetInt32("id_tipo_inmueble"),
            TipoInmueble = new TipoInmueble
            {
                Id = reader.GetInt32("id_tipo_inmueble"),
                Nombre = reader.GetString("TipoNombre")
            },
            Direccion = reader.GetString("direccion"),
            Cupo = reader.GetInt32("cupo"),
            Precio = reader.GetDecimal("precio"),
            Coord = reader.IsDBNull(reader.GetOrdinal("coord")) ? null : reader.GetString("coord"),
            Foto_portada = reader.IsDBNull(reader.GetOrdinal("foto_portada")) ? null : reader.GetString("foto_portada"),
            Fotos = reader.IsDBNull(reader.GetOrdinal("fotos")) ? null : reader.GetString("fotos"),
            Activo = reader.GetBoolean("activo"),
            Reservas = new List<Reserva>()
        };
    }
}