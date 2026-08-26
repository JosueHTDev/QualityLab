using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.Entities
{
    public class Analisis
    {
        public int Id { get; set; }

        public int MuestraId { get; set; }
        public Muestra? Muestra { get; set; }

        public int TecnicoId { get; set; }
        public Tecnico? Tecnico { get; set; }

        public string TipoAnalisis { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin { get; set; }
        public EstadoAnalisis Estado { get; set; } = EstadoAnalisis.Pendiente;
        public string Observaciones { get; set; } = string.Empty;

        public ICollection<Resultado> Resultados { get; set; } = new List<Resultado>();
    }
}
