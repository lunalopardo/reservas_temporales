using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories
{
    public class RepositorioReserva : RepositorioBase
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration) { }

        public IList<Reserva> GetPaginado(string? buscar = null, bool? soloVigentes = null, int? diasParaTerminar = null, int paginaNro = 1, int tamPagina = 10)
        {
            IList<Reserva> listado = new List<Reserva>();
            int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
            var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"
            SELECT r.id, r.id_inmueble, r.id_inquilino, r.fecha_desde, r.fecha_hasta, r.monto_diario, r.fecha_terminacion_anticipada, r.multa, r.creado_por_user_id, r.terminado_por_user_id, r.activo,
                   i.id AS inmueble_id, i.direccion AS inmueble_direccion, i.precio AS inmueble_precio, i.activo AS inmueble_activo,
                   iq.id AS inquilino_id, iq.nombre AS inquilino_nombre, iq.apellido AS inquilino_apellido, iq.dni AS inquilino_dni, iq.email AS inquilino_email, iq.telefono AS inquilino_telefono, iq.activo AS inquilino_activo
            FROM Reserva r
            INNER JOIN Inmueble i ON r.id_inmueble = i.id
            INNER JOIN Inquilino iq ON r.id_inquilino = iq.id
            WHERE r.activo = 1 
              AND (@buscar IS NULL OR i.direccion LIKE @buscar 
                                   OR iq.nombre LIKE @buscar 
                                   OR iq.apellido LIKE @buscar 
                                   OR iq.dni LIKE @buscar)
              AND (@soloVigentes IS NULL OR @soloVigentes = 0 OR (CURRENT_DATE() BETWEEN r.fecha_desde AND COALESCE(r.fecha_terminacion_anticipada, r.fecha_hasta)))
              AND (@diasParaTerminar IS NULL OR (COALESCE(r.fecha_terminacion_anticipada, r.fecha_hasta) BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL @diasParaTerminar DAY)))
            ORDER BY r.id DESC
            LIMIT @limit OFFSET @offset";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@buscar", paramBuscar);
                    command.Parameters.AddWithValue("@soloVigentes", (object?)soloVigentes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@diasParaTerminar", (object?)diasParaTerminar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@limit", tamPagina);
                    command.Parameters.AddWithValue("@offset", offset);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listado.Add(ParseReserva(reader));
                        }
                    }
                }
            }
            return listado;
        }

        public int ObtenerCantidad(string? buscar = null, bool? soloVigentes = null, int? diasParaTerminar = null)
        {
            int total = 0;
            var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"
            SELECT COUNT(r.id) 
            FROM Reserva r
            INNER JOIN Inmueble i ON r.id_inmueble = i.id
            INNER JOIN Inquilino iq ON r.id_inquilino = iq.id
            WHERE r.activo = 1 
              AND (@buscar IS NULL OR i.direccion LIKE @buscar 
                                   OR iq.nombre LIKE @buscar 
                                   OR iq.apellido LIKE @buscar 
                                   OR iq.dni LIKE @buscar)
              AND (@soloVigentes IS NULL OR @soloVigentes = 0 OR (CURRENT_DATE() BETWEEN r.fecha_desde AND COALESCE(r.fecha_terminacion_anticipada, r.fecha_hasta)))
              AND (@diasParaTerminar IS NULL OR (COALESCE(r.fecha_terminacion_anticipada, r.fecha_hasta) BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL @diasParaTerminar DAY)))";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@buscar", paramBuscar);
                    command.Parameters.AddWithValue("@soloVigentes", (object?)soloVigentes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@diasParaTerminar", (object?)diasParaTerminar ?? DBNull.Value);

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

        public List<Reserva> GetAllActivas()
        {
            var listado = new List<Reserva>();
            var query = @"
                SELECT r.id, r.id_inmueble, r.id_inquilino, r.fecha_desde, r.fecha_hasta, r.monto_diario, r.fecha_terminacion_anticipada, r.multa, r.creado_por_user_id, r.terminado_por_user_id, r.activo,
                       i.id AS inmueble_id, i.direccion AS inmueble_direccion, i.precio AS inmueble_precio, i.activo AS inmueble_activo,
                       iq.id AS inquilino_id, iq.nombre AS inquilino_nombre, iq.apellido AS inquilino_apellido, iq.dni AS inquilino_dni, iq.email AS inquilino_email, iq.telefono AS inquilino_telefono, iq.activo AS inquilino_activo
                FROM Reserva r
                INNER JOIN Inmueble i ON r.id_inmueble = i.id
                INNER JOIN Inquilino iq ON r.id_inquilino = iq.id
                WHERE r.activo = 1";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(ParseReserva(reader));
            }

            return listado;
        }

        public Reserva? GetById(int id)
        {
            var query = @"
                        SELECT r.id, r.id_inmueble, r.id_inquilino, r.fecha_desde, r.fecha_hasta, 
                            r.fecha_terminacion_anticipada, r.multa, r.monto_diario, 
                            r.creado_por_user_id, r.terminado_por_user_id, r.activo,
                            i.id AS inmueble_id, i.direccion AS inmueble_direccion, i.precio AS inmueble_precio, i.activo AS inmueble_activo,
                            iq.id AS inquilino_id, iq.nombre AS inquilino_nombre, iq.apellido AS inquilino_apellido, iq.dni AS inquilino_dni, iq.email AS inquilino_email, iq.telefono AS inquilino_telefono, iq.activo AS inquilino_activo
                        FROM Reserva r
                        INNER JOIN Inmueble i ON r.id_inmueble = i.id
                        INNER JOIN Inquilino iq ON r.id_inquilino = iq.id
                        WHERE r.id = @id";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return ParseReserva(reader);
            }

            return null;
        }

        public bool ExisteSuperposicion(int idInmueble, DateTime fechaDesde, DateTime fechaHasta, int? idReservaExcluida = null)
        {
            var query = @"
                SELECT COUNT(*) FROM Reserva 
                WHERE id_inmueble = @idInmueble 
                AND activo = 1 
                AND (@idReservaExcluida IS NULL OR id != @idReservaExcluida)
                AND fecha_desde < @fechaHasta 
                AND fecha_hasta > @fechaDesde";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@idInmueble", idInmueble);
            command.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            command.Parameters.AddWithValue("@idReservaExcluida", (object?)idReservaExcluida ?? DBNull.Value);

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public int Create(Reserva reserva)
        {
            string query = @"INSERT INTO Reserva 
            (id_inmueble, id_inquilino, fecha_desde, fecha_hasta, monto_diario, creado_por_user_id, activo)
            VALUES 
            (@idInmueble, @idInquilino, @fechaDesde, @fechaHasta, @montoDiario, @creadoPorUserId, 1);
            SELECT LAST_INSERT_ID();";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
            command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
            command.Parameters.AddWithValue("@fechaDesde", reserva.FechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", reserva.FechaHasta);
            command.Parameters.AddWithValue("@montoDiario", reserva.MontoDiario);
            command.Parameters.AddWithValue("@creadoPorUserId", (object?)reserva.CreadoPorUserId ?? DBNull.Value);

            connection.Open();
            reserva.Id = Convert.ToInt32(command.ExecuteScalar());
            return reserva.Id;
        }

        public int Update(Reserva reserva)
        {
            string query = @"
            UPDATE Reserva 
            SET id_inmueble = @idInmueble, 
                id_inquilino = @idInquilino, 
                fecha_desde = @fechaDesde, 
                fecha_hasta = @fechaHasta, 
                monto_diario = @montoDiario 
            WHERE id = @id;";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
            command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
            command.Parameters.AddWithValue("@fechaDesde", reserva.FechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", reserva.FechaHasta);
            command.Parameters.AddWithValue("@montoDiario", reserva.MontoDiario);
            command.Parameters.AddWithValue("@id", reserva.Id);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int FinalizarAnticipadamente(int idReserva, DateTime fechaTerminacion, decimal multa, int terminadoPorUserId)
        {
            string query = @"UPDATE Reserva SET 
                            fecha_terminacion_anticipada = @fechaTerminacion, 
                            multa = @multa,
                            terminado_por_user_id = @terminadoPorUserId,
                            activo = 1
                            WHERE id = @id;";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@fechaTerminacion", fechaTerminacion);
            command.Parameters.AddWithValue("@multa", multa);
            command.Parameters.AddWithValue("@terminadoPorUserId", terminadoPorUserId);
            command.Parameters.AddWithValue("@id", idReserva);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public bool Exists(int id)
        {
            var query = "SELECT COUNT(*) FROM Reserva WHERE id = @id";
            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public int DeleteLogico(int id, int? terminadoPorUserId = null)
        {
            var query = "UPDATE Reserva SET activo = 0, terminado_por_user_id = @terminadoPorUserId WHERE id = @id";
            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@terminadoPorUserId", (object?)terminadoPorUserId ?? DBNull.Value);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public List<object> GetFechasOtras(int idInmueble, int? idReserva = null)
        {
            var listado = new List<object>();
            var query = @"
                SELECT fecha_desde, fecha_hasta FROM Reserva 
                WHERE id_inmueble = @idInmueble 
                AND activo = 1 
                AND (@idReserva IS NULL OR id != @idReserva)";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@idInmueble", idInmueble);
            command.Parameters.AddWithValue("@idReserva", (object?)idReserva ?? DBNull.Value);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(new
                {
                    fechaDesde = reader.GetDateTime("fecha_desde").ToString("yyyy-MM-dd"),
                    fechaHasta = reader.GetDateTime("fecha_hasta").ToString("yyyy-MM-dd")
                });
            }

            return listado;
        }

        public object? GetFechaActual(int idReserva, int idInmueble)
        {
            var query = @"
                SELECT fecha_desde, fecha_hasta FROM Reserva 
                WHERE id = @idReserva 
                AND id_inmueble = @idInmueble 
                AND activo = 1";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@idReserva", idReserva);
            command.Parameters.AddWithValue("@idInmueble", idInmueble);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new
                {
                    fechaDesde = reader.GetDateTime("fecha_desde").ToString("yyyy-MM-dd"),
                    fechaHasta = reader.GetDateTime("fecha_hasta").ToString("yyyy-MM-dd")
                };
            }

            return null;
        }

        public Inmueble? GetInmuebleParaReserva(int idInmueble)
        {
            var query = "SELECT id, direccion, precio, activo FROM Inmueble WHERE id = @id";
            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", idInmueble);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Inmueble
                {
                    Id = reader.GetInt32("id"),
                    Direccion = reader.GetString("direccion"),
                    Precio = reader.GetDecimal("precio"),
                    Activo = reader.GetBoolean("activo")
                };
            }

            return null;
        }


        public decimal? GetPrecioInmueble(int id)
        {
            var query = "SELECT precio FROM Inmueble WHERE id = @id AND activo = 1";
            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();

            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDecimal(result);
            }

            return null;
        }

        private Reserva ParseReserva(MySqlDataReader reader)
        {
            return new Reserva
            {
                Id = reader.GetInt32("id"),
                IdInmueble = reader.GetInt32("id_inmueble"),
                IdInquilino = reader.GetInt32("id_inquilino"),
                FechaDesde = reader.GetDateTime("fecha_desde"),
                FechaHasta = reader.GetDateTime("fecha_hasta"),
                FechaTerminacionAnticipada = reader.IsDBNull(reader.GetOrdinal("fecha_terminacion_anticipada"))
                    ? null
                    : reader.GetDateTime("fecha_terminacion_anticipada"),

                Multa = reader.IsDBNull(reader.GetOrdinal("multa"))
                    ? null
                    : reader.GetDecimal("multa"),

                MontoDiario = reader.GetDecimal("monto_diario"),
                CreadoPorUserId = reader.GetInt32("creado_por_user_id"),
                TerminadoPorUserId = reader.IsDBNull(reader.GetOrdinal("terminado_por_user_id"))
                    ? null
                    : reader.GetInt32("terminado_por_user_id"),

                Activo = reader.GetBoolean("activo"),
                Inmueble = new Inmueble
                {
                    Id = reader.GetInt32("inmueble_id"),
                    Direccion = reader.GetString("inmueble_direccion"),
                    Precio = reader.GetDecimal("inmueble_precio"),
                    Activo = reader.GetBoolean("inmueble_activo")
                },
                Inquilino = new Inquilino
                {
                    IdInquilino = reader.GetInt32("inquilino_id"),
                    Nombre = reader.GetString("inquilino_nombre"),
                    Apellido = reader.GetString("inquilino_apellido"),
                    Dni = reader.GetString("inquilino_dni"),
                    Email = reader.GetString("inquilino_email"),
                    Telefono = reader.GetString("inquilino_telefono"),
                    Activo = reader.GetBoolean("inquilino_activo")
                }
            };
        }

        public class RangoFechasDto
        {
            public DateTime FechaDesde { get; set; }
            public DateTime FechaHasta { get; set; }
        }

        public List<RangoFechasDto> ObtenerFechasReservadasPorInmueble(int idInmueble)
        {
            var lista = new List<RangoFechasDto>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT fecha_desde, fecha_hasta FROM Reserva WHERE id_inmueble = @id AND activo = 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", idInmueble);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new RangoFechasDto
                            {
                                FechaDesde = reader.GetDateTime("fecha_desde"),
                                FechaHasta = reader.GetDateTime("fecha_hasta")
                            });
                        }
                    }
                }
            }
            return lista;
        }

        //Método para traer la reserva al select de Pagos tanto como por inquilino como por inm.
        public List<object> BuscarPorInquilinoODireccion(string q)
        {
            var lista = new List<object>();
            var paramBuscar = string.IsNullOrWhiteSpace(q) ? (object)DBNull.Value : $"%{q.Trim()}%";

            string sql = @"
        SELECT r.id AS IdReserva, 
               CONCAT(iq.nombre, ' ', iq.apellido) AS InquilinoNombreCompleto, 
               i.direccion AS InmuebleDireccion
        FROM Reserva r
        INNER JOIN Inquilino iq ON r.id_inquilino = iq.id
        INNER JOIN Inmueble i ON r.id_inmueble = i.id
        WHERE r.activo = 1 
          AND (@buscar IS NULL OR iq.nombre LIKE @buscar OR iq.apellido LIKE @buscar OR i.direccion LIKE @buscar)
        LIMIT 20;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@buscar", paramBuscar);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                id = reader.GetInt32("IdReserva"),
                                texto = $"{reader.GetString("InquilinoNombreCompleto")} - {reader.GetString("InmuebleDireccion")}"
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public IList<int> ObtenerIdsInmueblesOcupados(DateTime fechaDesde, DateTime fechaHasta)
        {
            var idsOcupados = new List<int>();
            var query = @"
                        SELECT DISTINCT id_inmueble 
                        FROM Reserva 
                        WHERE activo = 1 
                        AND fecha_desde < @fechaHasta 
                        AND fecha_hasta > @fechaDesde";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                idsOcupados.Add(reader.GetInt32("id_inmueble"));
            }

            return idsOcupados;
        }
    }
}