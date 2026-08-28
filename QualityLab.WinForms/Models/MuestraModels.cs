namespace QualityLab.WinForms.Models
{
    public class MuestraCreateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public int LoteId { get; set; }
        public int ClienteId { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
    }

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

        public override string ToString() => $"{Codigo} [{Estado}]";
    }

    public class AsignarTecnicoDto
    {
        public int TecnicoId { get; set; }
    }

    public class AnalisisCreateDto
    {
        public int MuestraId { get; set; }
        public int TecnicoId { get; set; }
        public string TipoAnalisis { get; set; } = string.Empty;
    }

    public class AnalisisResponse
    {
        public int Id { get; set; }
        public int MuestraId { get; set; }
        public string TipoAnalisis { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public override string ToString() => $"#{Id} - {TipoAnalisis} [{Estado}]";
    }

    public class CompletarAnalisisDto
    {
        public string Observaciones { get; set; } = string.Empty;
    }
    public class EstadoMuestraDto
    {
        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool TieneCertificado { get; set; }
    }
}
