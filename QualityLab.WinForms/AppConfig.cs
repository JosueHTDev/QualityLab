namespace QualityLab.WinForms
{
    /// <summary>
    /// Configuración de la aplicación. Ajustar BaseUrl si la API corre en
    /// otro puerto/host (revisa la consola de QualityLab.API al ejecutarla).
    /// </summary>
    public static class AppConfig
    {
        public const string BaseUrl = "http://localhost:5080/";

        // Identifica esta aplicación ante la API (ver RequestTrackingMiddleware).
        public const string ClientAppName = "QualityLab.WinForms";
    }
}
