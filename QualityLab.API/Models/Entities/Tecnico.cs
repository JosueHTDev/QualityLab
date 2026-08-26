namespace QualityLab.API.Models.Entities
{
    public class Tecnico
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        public ICollection<Muestra> MuestrasAsignadas { get; set; } = new List<Muestra>();
        public ICollection<Analisis> Analisis { get; set; } = new List<Analisis>();
        public ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
        public ICollection<AvanceMuestra> Avances { get; set; } = new List<AvanceMuestra>();
    }
}
