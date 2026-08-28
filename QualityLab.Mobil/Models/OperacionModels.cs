using SQLite;

namespace QualityLab.Mobil.Models
{
    public class RegistrarAvanceDto
    {
        public int MuestraId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int PorcentajeAvance { get; set; }
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }

    public class RegistrarIncidenciaDto
    {
        public int MuestraId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }

    public enum TipoOperacion
    {
        Avance,
        Incidencia
    }

    public class OperacionPendiente
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid IdLocalOrigen { get; set; } = Guid.NewGuid();
        public TipoOperacion Tipo { get; set; }
        public int MuestraId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int PorcentajeAvance { get; set; }
        public DateTime FechaCreacionLocal { get; set; } = DateTime.UtcNow;

        public bool Sincronizado { get; set; } = false;
        public string? ErrorUltimoIntento { get; set; }
    }
}