namespace QualityLab.API.Models.Entities
{
    /// <summary>
    /// Incidencia registrada por un técnico (típicamente desde la app móvil .NET MAUI)
    /// cuando ocurre un problema con una muestra durante el análisis.
    /// </summary>
    public class Incidencia
    {
        public int Id { get; set; }

        public int MuestraId { get; set; }
        public Muestra? Muestra { get; set; }

        public int TecnicoId { get; set; }
        public Tecnico? Tecnico { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Resuelta { get; set; } = false;

        // Soporte para sincronización offline-first desde la app móvil:
        // el cliente genera este Id localmente (GUID) antes de tener conexión.
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }
}
