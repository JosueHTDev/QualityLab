namespace QualityLab.Mobil.Models
{
    public class MuestraResponse
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int LoteId { get; set; }
        public int ClienteId { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        public DateTime FechaRecepcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? TecnicoAsignadoId { get; set; }
        public string? TecnicoAsignadoNombre { get; set; }
    }
}