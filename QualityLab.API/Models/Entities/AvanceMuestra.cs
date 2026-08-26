namespace QualityLab.API.Models.Entities
{
    /// <summary>
    /// Registro de avance de una muestra, hecho por el técnico desde la app móvil.
    /// IdLocalOrigen permite que la app móvil trabaje sin conexión (offline-first)
    /// y luego sincronice sin duplicar registros (Prueba 8 - Sincronización posterior).
    /// </summary>
    public class AvanceMuestra
    {
        public int Id { get; set; }

        public int MuestraId { get; set; }
        public Muestra? Muestra { get; set; }

        public int TecnicoId { get; set; }
        public Tecnico? Tecnico { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public int PorcentajeAvance { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Idempotencia para sincronización offline (ver Middleware/README)
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }
}
