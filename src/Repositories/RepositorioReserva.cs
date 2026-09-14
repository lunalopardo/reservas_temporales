using MySqlConnector;
using ReservasTemporales.Models;

namespace ReservasTemporales.Repositories
{
    public class RepositorioReserva : RepositorioBase
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration) { }

        public List<Reserva> GetAllActivas()
        {
            var listado = new List<Reserva>();
            var query = @"
                SELECT r.id, r.id_inmueble, r.id_inquilino, r.fecha_desde, r.fecha_hasta, r.monto_diario, r.creado_por_user_id, r.terminado_por_user_id, r.activo,
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
                SELECT r.id, r.id_inmueble, r.id_inquilino, r.fecha_desde, r.fecha_hasta, r.monto_diario, r.creado_por_user_id, r.terminado_por_user_id, r.activo,
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
            var query = @"
                INSERT INTO Reserva (id_inmueble, id_inquilino, fecha_desde, fecha_hasta, monto_diario, creado_por_user_id, activo) 
                VALUES (@idInmueble, @idInquilino, @fechaDesde, @fechaHasta, @montoDiario, @creadoPorUserId, 1)";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
            command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
            command.Parameters.AddWithValue("@fechaDesde", reserva.FechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", reserva.FechaHasta);
            command.Parameters.AddWithValue("@montoDiario", reserva.MontoDiario);
            command.Parameters.AddWithValue("@creadoPorUserId", (object?)reserva.CreadoPorUserId ?? DBNull.Value);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int Update(Reserva reserva)
        {
            var query = @"
                UPDATE Reserva 
                SET id_inmueble = @idInmueble, 
                    id_inquilino = @idInquilino, 
                    fecha_desde = @fechaDesde, 
                    fecha_hasta = @fechaHasta,
                    monto_diario = @montoDiario
                WHERE id = @id";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", reserva.Id);
            command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
            command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
            command.Parameters.AddWithValue("@fechaDesde", reserva.FechaDesde);
            command.Parameters.AddWithValue("@fechaHasta", reserva.FechaHasta);
            command.Parameters.AddWithValue("@montoDiario", reserva.MontoDiario);

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

        public List<Inmueble> GetInmueblesDisponibles()
        {
            var listado = new List<Inmueble>();
            var query = "SELECT id, direccion, precio, activo FROM Inmueble WHERE activo = 1 ORDER BY direccion";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(new Inmueble
                {
                    Id = reader.GetInt32("id"),
                    Direccion = reader.GetString("direccion"),
                    Precio = reader.GetDecimal("precio"),
                    Activo = reader.GetBoolean("activo")
                });
            }

            return listado.Where(i => i.EstaDisponibleHoy).ToList();
        }

        public List<Inquilino> GetInquilinosActivos()
        {
            var listado = new List<Inquilino>();
            var query = "SELECT id, nombre, apellido, dni, email, telefono, activo FROM Inquilino WHERE activo = 1 ORDER BY apellido, nombre";

            using MySqlConnection connection = new(connectionString);
            using MySqlCommand command = new(query, connection);
            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                listado.Add(new Inquilino
                {
                    IdInquilino = reader.GetInt32("id"),
                    Nombre = reader.GetString("nombre"),
                    Apellido = reader.GetString("apellido"),
                    Dni = reader.GetString("dni"),
                    Email = reader.GetString("email"),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                    Activo = reader.GetBoolean("activo")
                });
            }

            return listado;
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

        private static Reserva ParseReserva(MySqlDataReader reader)
        {
            return new Reserva
            {
                Id = reader.GetInt32("id"),
                IdInmueble = reader.GetInt32("id_inmueble"),
                IdInquilino = reader.GetInt32("id_inquilino"),
                FechaDesde = reader.GetDateTime("fecha_desde"),
                FechaHasta = reader.GetDateTime("fecha_hasta"),
                MontoDiario = reader.GetDecimal("monto_diario"),
                CreadoPorUserId = reader.IsDBNull(reader.GetOrdinal("creado_por_user_id")) ? null : reader.GetInt32("creado_por_user_id"),
                TerminadoPorUserId = reader.IsDBNull(reader.GetOrdinal("terminado_por_user_id")) ? null : reader.GetInt32("terminado_por_user_id"),
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
                    Telefono = reader.IsDBNull(reader.GetOrdinal("inquilino_telefono")) ? string.Empty : reader.GetString("inquilino_telefono"),
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
    }
}