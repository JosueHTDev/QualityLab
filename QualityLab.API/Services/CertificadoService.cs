using System.Text;
using QualityLab.API.Models.Entities;

namespace QualityLab.API.Services
{
    /// <summary>
    /// Genera el certificado como documento de texto plano UTF-8.
    /// Se deja preparado para reemplazar esta implementación por una
    /// generación de PDF real (por ejemplo con QuestPDF) sin tocar el resto
    /// del código: solo cambia el contenido de GenerarContenidoCertificado.
    /// </summary>
    public class CertificadoService : ICertificadoService
    {
        public (string nombreArchivo, byte[] contenido) GenerarContenidoCertificado(
            Muestra muestra, IEnumerable<Resultado> resultados, string codigoCertificado)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine("        QUALITYLAB - CERTIFICADO DE ANALISIS");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Codigo de certificado : {codigoCertificado}");
            sb.AppendLine($"Fecha de emision       : {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine($"Muestra                : {muestra.Codigo}");
            sb.AppendLine($"Tipo de producto        : {muestra.TipoProducto}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("Resultados:");
            foreach (var r in resultados)
            {
                sb.AppendLine($"  - {r.Parametro}: {r.ValorObtenido} {r.Unidad} " +
                               $"(ref: {r.ValorReferencia}) => {(r.Conforme ? "CONFORME" : "NO CONFORME")}");
            }
            sb.AppendLine("==================================================");

            var nombreArchivo = $"Certificado_{muestra.Codigo}.txt";
            return (nombreArchivo, Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
