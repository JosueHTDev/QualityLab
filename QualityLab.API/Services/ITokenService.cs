using QualityLab.API.Models.Entities;

namespace QualityLab.API.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Genera un JWT firmado con las claims del usuario:
        /// identidad, rol y (si aplica) ClienteId/TecnicoId.
        /// Estas claims son las que luego se usan en los controladores
        /// para responder "¿cómo se autenticó?" y para filtrar datos sensibles.
        /// </summary>
        (string token, DateTime expiraEn) GenerarToken(Usuario usuario);
    }
}
