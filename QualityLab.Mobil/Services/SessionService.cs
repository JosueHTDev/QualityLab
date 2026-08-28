using QualityLab.Mobil.Models;

namespace QualityLab.Mobil.Services
{
    public class SessionService
    {
        private const string ClavePrefijo = "QualityLab_";

        public string Token
        {
            get => Preferences.Get(ClavePrefijo + "Token", string.Empty);
            private set => Preferences.Set(ClavePrefijo + "Token", value);
        }

        public DateTime ExpiraEn
        {
            get => Preferences.Get(ClavePrefijo + "ExpiraEn", DateTime.MinValue);
            private set => Preferences.Set(ClavePrefijo + "ExpiraEn", value);
        }

        public string NombreUsuario
        {
            get => Preferences.Get(ClavePrefijo + "NombreUsuario", string.Empty);
            private set => Preferences.Set(ClavePrefijo + "NombreUsuario", value);
        }

        public int TecnicoId
        {
            get => Preferences.Get(ClavePrefijo + "TecnicoId", 0);
            private set => Preferences.Set(ClavePrefijo + "TecnicoId", value);
        }

        public bool EstaAutenticado => !string.IsNullOrEmpty(Token) && DateTime.UtcNow < ExpiraEn;

        public void IniciarSesion(LoginResponse respuesta)
        {
            Token = respuesta.Token;
            ExpiraEn = respuesta.ExpiraEn;
            NombreUsuario = respuesta.NombreUsuario;
            TecnicoId = respuesta.TecnicoId ?? 0;
        }

        public void CerrarSesion()
        {
            Preferences.Clear();
        }
    }
}