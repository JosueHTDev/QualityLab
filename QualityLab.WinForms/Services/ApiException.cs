namespace QualityLab.WinForms.Services
{
    /// <summary>
    /// Excepción lanzada por ApiClient cuando la API responde con un código
    /// de error (401, 403, 404, 400, 500, etc). El mensaje ya viene listo
    /// para mostrarlo directamente en un MessageBox.
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string mensaje) : base(mensaje)
        {
            StatusCode = statusCode;
        }
    }
}
