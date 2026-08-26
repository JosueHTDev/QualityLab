using System.ComponentModel.DataAnnotations;

namespace QualityLab.API.Models.DTOs
{
    public class ResultadoCreateDto
    {
        [Required] public int AnalisisId { get; set; }
        [Required] public string Parametro { get; set; } = string.Empty;
        [Required] public string ValorObtenido { get; set; } = string.Empty;
        public string ValorReferencia { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        [Required] public bool Conforme { get; set; }
    }

    public class ResultadoResponseDto
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

    public class CertificadoResponseDto
    {
        public int Id { get; set; }
        public int MuestraId { get; set; }
        public string CodigoCertificado { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class EstadoMuestraDto
    {
        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool TieneCertificado { get; set; }
    }
}
