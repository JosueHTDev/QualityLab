using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.Entities
{
    public class Certificado
    {
        public int Id { get; set; }

        public int MuestraId { get; set; }
        public Muestra? Muestra { get; set; }

        public string CodigoCertificado { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
        public EstadoCertificado Estado { get; set; } = EstadoCertificado.Emitido;

        // Contenido del PDF generado, almacenado directamente en SQL Server (varbinary(max))
        public byte[] ContenidoArchivo { get; set; } = Array.Empty<byte>();
        public string NombreArchivo { get; set; } = string.Empty;

        public int GeneradoPorUsuarioId { get; set; }
    }
}
