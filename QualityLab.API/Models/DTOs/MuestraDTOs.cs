using System.ComponentModel.DataAnnotations;
using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.DTOs
{
    public class LoteCreateDto
    {
        [Required] public string Codigo { get; set; } = string.Empty;
        [Required] public int ClienteId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class LoteResponseDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string? ClienteRazonSocial { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class MuestraCreateDto
    {
        [Required] public string Codigo { get; set; } = string.Empty;
        [Required] public int LoteId { get; set; }
        [Required] public int ClienteId { get; set; }
        [Required] public string TipoProducto { get; set; } = string.Empty;
    }

    public class MuestraResponseDto
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

    public class AsignarTecnicoDto
    {
        [Required] public int TecnicoId { get; set; }
    }

    public class AnalisisCreateDto
    {
        [Required] public int MuestraId { get; set; }
        [Required] public int TecnicoId { get; set; }
        [Required] public string TipoAnalisis { get; set; } = string.Empty;
    }

    public class AnalisisResponseDto
    {
        public int Id { get; set; }
        public int MuestraId { get; set; }
        public string TipoAnalisis { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    public class CompletarAnalisisDto
    {
        public string Observaciones { get; set; } = string.Empty;
    }
}
