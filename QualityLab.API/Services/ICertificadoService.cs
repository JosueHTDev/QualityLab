using QualityLab.API.Models.Entities;

namespace QualityLab.API.Services
{
    public interface ICertificadoService
    {
        /// <summary>
        /// Genera el contenido del certificado para una muestra ya analizada.
        /// Devuelve el nombre de archivo sugerido y el contenido en bytes.
        /// </summary>
        (string nombreArchivo, byte[] contenido) GenerarContenidoCertificado(
            Muestra muestra, IEnumerable<Resultado> resultados, string codigoCertificado);
    }
}
