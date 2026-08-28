namespace QualityLab.Web.Services
{
    /// <summary>
    /// Excepción lanzada por ApiClient cuando QualityLab.API responde con error.
    /// El mensaje ya viene listo para mostrarlo en la vista.
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
