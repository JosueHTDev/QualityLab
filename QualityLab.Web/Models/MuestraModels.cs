namespace QualityLab.Web.Models
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

    public class EstadoMuestraResponse
    {
        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool TieneCertificado { get; set; }
    }

    public class ResultadoResponse
    {
        public int Id { get; set; }
        public int AnalisisId { get; set; }
        public int MuestraId { get; set; }
        public string Parametro { get; set; } = string.Empty;
        public string ValorObtenido { get; set; } = string.Empty;
        public string ValorReferencia { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public bool Conforme { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>
    /// ViewModel que combina la información de la muestra + su estado + sus
    /// resultados (si ya están disponibles), para pintar todo en una sola vista.
    /// </summary>
    public class MuestraDetalleViewModel
    {
        public MuestraResponse Muestra { get; set; } = new();
        public EstadoMuestraResponse Estado { get; set; } = new();
        public List<ResultadoResponse> Resultados { get; set; } = new();
    }
}
