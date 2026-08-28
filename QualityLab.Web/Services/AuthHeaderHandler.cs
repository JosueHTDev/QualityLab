using System.Net.Http.Headers;

namespace QualityLab.Web.Services
{
    /// <summary>
    /// Se ejecuta antes de cada llamada a la API. Toma el JWT que se guardó
    /// como claim en la cookie de autenticación (al hacer login) y lo reenvía
    /// como Authorization: Bearer. También identifica esta aplicación ante
    /// la API con "X-Client-App" (ver RequestTrackingMiddleware del API).
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("ApiToken")?.Value;

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Headers.Add("X-Client-App", "QualityLab.Web");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
