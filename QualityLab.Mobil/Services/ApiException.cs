namespace QualityLab.Mobil.Services
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string mensaje) : base(mensaje)
        {
            StatusCode = statusCode;
        }
    }
}