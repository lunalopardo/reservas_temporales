namespace ReservasTemporales.Models
{
    public class BusquedaInmuebleViewModel
    {
        public int? IdTipoInmueble { get; set; }
        public int? Personas { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public IEnumerable<Inmueble> Resultados { get; set; } = new List<Inmueble>();
    }
}