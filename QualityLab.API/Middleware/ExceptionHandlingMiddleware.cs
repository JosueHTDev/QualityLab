using System.Net;
using System.Text.Json;

namespace QualityLab.API.Middleware
{
    /// <summary>
    /// Captura cualquier excepción no controlada y devuelve una respuesta
    /// JSON consistente en vez de un error 500 crudo. También registra
    /// el correlationId puesto por RequestTrackingMiddleware.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = context.Items.TryGetValue("CorrelationId", out var id) ? id : "N/A";
                _logger.LogError(ex, "[{CorrelationId}] Error no controlado", correlationId);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var respuesta = new
                {
                    mensaje = "Ocurrió un error inesperado en el servidor.",
                    correlationId
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
            }
        }
    }
}
