namespace QualityLab.WinForms.Services
{
    /// <summary>
    /// Sesión activa del usuario logueado. Estático y en memoria: vive mientras
    /// la aplicación WinForms está abierta (se pierde al cerrarla, como corresponde
    /// a un cliente de escritorio sin "recordarme").
    /// </summary>
    public static class SessionManager
    {
        public static string Token { get; private set; } = string.Empty;
        public static DateTime ExpiraEn { get; private set; }
        public static string NombreUsuario { get; private set; } = string.Empty;
        public static string Rol { get; private set; } = string.Empty;
        public static int? TecnicoId { get; private set; }
        public static int? ClienteId { get; private set; }

        public static bool EstaAutenticado => !string.IsNullOrEmpty(Token) && DateTime.UtcNow < ExpiraEn;

        public static void IniciarSesion(Models.LoginResponse respuesta)
        {
            Token = respuesta.Token;
            ExpiraEn = respuesta.ExpiraEn;
            NombreUsuario = respuesta.NombreUsuario;
            Rol = respuesta.Rol;
            TecnicoId = respuesta.TecnicoId;
            ClienteId = respuesta.ClienteId;
        }

        public static bool TieneRol(params string[] roles) =>
            roles.Any(r => string.Equals(r, Rol, StringComparison.OrdinalIgnoreCase));

        public static void CerrarSesion()
        {
            Token = string.Empty;
            NombreUsuario = string.Empty;
            Rol = string.Empty;
            TecnicoId = null;
            ClienteId = null;
        }
    }
}
