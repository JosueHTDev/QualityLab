using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.Entities
{
    public class Muestra
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;

        public int LoteId { get; set; }
        public Lote? Lote { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string TipoProducto { get; set; } = string.Empty;
        public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;

        public EstadoMuestra Estado { get; set; } = EstadoMuestra.Registrada;

        // Asignación de técnico (paso "Asignación técnico" del flujo de laboratorio)
        public int? TecnicoAsignadoId { get; set; }
        public Tecnico? TecnicoAsignado { get; set; }
        public DateTime? FechaAsignacion { get; set; }

        public ICollection<Analisis> Analisis { get; set; } = new List<Analisis>();
        public ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
        public ICollection<AvanceMuestra> Avances { get; set; } = new List<AvanceMuestra>();
        public Certificado? Certificado { get; set; }
    }
}
