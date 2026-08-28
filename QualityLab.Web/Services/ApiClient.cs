using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using QualityLab.Web.Models;

namespace QualityLab.Web.Services
{
    /// <summary>
    /// Cliente HTTP tipado hacia QualityLab.API. El token JWT y el header
    /// X-Client-App se agregan automáticamente vía AuthHeaderHandler.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<T?> GetAsync<T>(string ruta)
        {
            var response = await _http.GetAsync(ruta);
            await LanzarSiHayError(response);
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string ruta, object body)
        {
            var response = await _http.PostAsync(ruta, ContenidoJson(body));
            await LanzarSiHayError(response);
            if (response.StatusCode == HttpStatusCode.NoContent) return default;
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        /// <summary>Descarga un archivo (ej. certificado) y devuelve su contenido + nombre sugerido.</summary>
        public async Task<(byte[] contenido, string nombreArchivo, string contentType)> DescargarArchivoAsync(string ruta)
        {
            var response = await _http.GetAsync(ruta);
            await LanzarSiHayError(response);

            var nombreArchivo = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "certificado.txt";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var bytes = await response.Content.ReadAsByteArrayAsync();

            return (bytes, nombreArchivo, contentType);
        }

        private static StringContent ContenidoJson(object body) =>
            new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private static async Task LanzarSiHayError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string mensaje = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión no es válida o venció. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tienes permiso para acceder a esta información.",
                HttpStatusCode.NotFound => "No se encontró la información solicitada.",
                _ => "Ocurrió un error al comunicarse con el servidor."
            };

            try
            {
                var contenido = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(contenido))
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(contenido, _jsonOptions);
                    if (!string.IsNullOrWhiteSpace(error?.Mensaje)) mensaje = error!.Mensaje!;
                }
            }
            catch (JsonException)
            {
                // La respuesta no era JSON; se conserva el mensaje genérico.
            }

            throw new ApiException((int)response.StatusCode, mensaje);
        }
    }
}
