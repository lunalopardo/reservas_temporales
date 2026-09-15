using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories;

public class RepositorioInmueble : RepositorioBase
{
    public RepositorioInmueble(IConfiguration configuration) : base(configuration) { }

    public IList<Inmueble> GetPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Inmueble> listado = new List<Inmueble>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT i.*, 
                       p.nombre AS PropietarioNombre, p.apellido AS PropietarioApellido,
                       t.nombre AS TipoNombre
                FROM Inmueble i
                INNER JOIN Propietario p ON i.id_propietario = p.id
                INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                WHERE i.activo = 1 
                  AND (@buscar IS NULL OR i.direccion LIKE @buscar 
                                       OR p.nombre LIKE @buscar 
                                       OR p.apellido LIKE @buscar 
                                       OR t.nombre LIKE @buscar)
                ORDER BY i.id DESC
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
                        listado.Add(ParseInmueble(reader));
                    }
                }

                // Cargar reservas para evaluar la disponibilidad
                if (listado.Any())
                {
                    CargarReservasParaInmuebles(connection, listado);
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
                SELECT COUNT(i.id) 
                FROM Inmueble i
                INNER JOIN Propietario p ON i.id_propietario = p.id
                INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id
                WHERE i.activo = 1 
                  AND (@buscar IS NULL OR i.direccion LIKE @buscar 
                                       OR p.nombre LIKE @buscar 
                                       OR p.apellido LIKE @buscar 
                                       OR t.nombre LIKE @buscar)";

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
                      (id_propietario, id_tipo_inmueble, direccion, cupo, coord, precio, porcentaje_sena, foto_portada, fotos, activo)
                      VALUES 
                      (@idPropietario, @idTipoInmueble, @direccion, @cupo, @coord, @precio, @porcentajeSena, @fotoPortada, @fotos, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@idPropietario", inmueble.IdPropietario);
        command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
        command.Parameters.AddWithValue("@direccion", inmueble.Direccion);
        command.Parameters.AddWithValue("@cupo", inmueble.Cupo);
        command.Parameters.AddWithValue("@coord", (object?)inmueble.Coord ?? DBNull.Value);
        command.Parameters.AddWithValue("@precio", inmueble.Precio);
        command.Parameters.AddWithValue("@porcentajeSena", inmueble.PorcentajeSena);
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
                      porcentaje_sena = @porcentajeSena,
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
        command.Parameters.AddWithValue("@porcentajeSena", inmueble.PorcentajeSena);
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

    private void CargarReservasParaInmuebles(MySqlConnection connection, IList<Inmueble> inmuebles)
    {
        var ids = inmuebles.Select(i => i.Id).ToList();
        var idsParam = string.Join(",", ids);

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

    public IList<Inmueble> GetParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Inmueble>();

        string sql = @"
        SELECT id, direccion 
        FROM Inmueble 
        WHERE activo = 1";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            sql += " AND direccion LIKE @buscar";
        }

        sql += " ORDER BY direccion ASC LIMIT @limite";

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
                        listado.Add(new Inmueble
                        {
                            Id = reader.GetInt32("id"),
                            Direccion = reader.GetString("direccion")
                        });
                    }
                }
            }
        }

        return listado;
    }

    internal static Inmueble ParseInmueble(MySqlDataReader reader)
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
            PorcentajeSena = reader.GetDecimal("porcentaje_sena"),
            Coord = reader.IsDBNull(reader.GetOrdinal("coord")) ? null : reader.GetString("coord"),
            Foto_portada = reader.IsDBNull(reader.GetOrdinal("foto_portada")) ? null : reader.GetString("foto_portada"),
            Fotos = reader.IsDBNull(reader.GetOrdinal("fotos")) ? null : reader.GetString("fotos"),
            Activo = reader.GetBoolean("activo"),
            Reservas = new List<Reserva>()
        };
    }
}