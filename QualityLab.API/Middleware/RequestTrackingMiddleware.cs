using System.Diagnostics;

namespace QualityLab.API.Middleware
{
    /// <summary>
    /// Middleware que responde a las preguntas de trazabilidad exigidas por el caso:
    ///   - ¿Qué aplicación hizo la petición?  -> header "X-Client-App" (WinForms/Web/MAUI)
    ///   - ¿Qué API recibió la petición?      -> nombre de esta API + ruta invocada
    ///   - ¿Qué middleware intervino?          -> este middleware deja constancia en el log
    ///
    /// Cada cliente (WinForms, ASP.NET Core MVC, MAUI) debe enviar el header
    /// "X-Client-App" con su nombre, por ejemplo:
    ///   X-Client-App: QualityLab.WinForms
    ///   X-Client-App: QualityLab.Web
    ///   X-Client-App: QualityLab.Mobile
    ///
    /// La respuesta incluye un header "X-Correlation-Id" que permite rastrear
    /// la petición de punta a punta en los logs (Prueba 9 - Middleware funcionando).
    /// </summary>
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTrackingMiddleware> _logger;
        private const string ApiName = "QualityLab.API";

        public RequestTrackingMiddleware(RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existing)
                ? existing.ToString()
                : Guid.NewGuid().ToString();

            var clientApp = context.Request.Headers.TryGetValue("X-Client-App", out var app)
                ? app.ToString()
                : "Desconocido";

            context.Items["ClientApp"] = clientApp;
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            context.Response.Headers["X-Api-Name"] = ApiName;

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Peticion recibida | App cliente: {ClientApp} | API: {ApiName} | Ruta: {Metodo} {Ruta}",
                correlationId, clientApp, ApiName, context.Request.Method, context.Request.Path);

            await _next(context);

            stopwatch.Stop();

            var usuario = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity!.Name
                : "anonimo";

            _logger.LogInformation(
                "[{CorrelationId}] Peticion finalizada | Usuario: {Usuario} | StatusCode: {StatusCode} | Duracion: {Duracion}ms",
                correlationId, usuario, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
