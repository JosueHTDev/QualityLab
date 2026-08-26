using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.Entities
{
    public class Lote
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;
        public string Descripcion { get; set; } = string.Empty;
        public EstadoLote Estado { get; set; } = EstadoLote.Recibido;

        public ICollection<Muestra> Muestras { get; set; } = new List<Muestra>();
    }
}
