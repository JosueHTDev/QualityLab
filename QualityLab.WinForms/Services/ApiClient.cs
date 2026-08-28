using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using QualityLab.WinForms.Models;

namespace QualityLab.WinForms.Services
{
    /// <summary>
    /// Punto único de comunicación con QualityLab.API. Agrega automáticamente:
    ///   - El token JWT (Authorization: Bearer ...) si hay sesión activa.
    ///   - El header "X-Client-App" para que la API sepa qué aplicación llamó
    ///     (ver RequestTrackingMiddleware en el API).
    /// Traduce cualquier respuesta de error en una ApiException con el
    /// mensaje que ya envía la API, lista para mostrar en un MessageBox.
    /// </summary>
    public class ApiClient
    {
        private static readonly HttpClient _http = CrearHttpClient();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static HttpClient CrearHttpClient()
        {
            var handler = new HttpClientHandler();

            // Solo para desarrollo local con certificado autofirmado (https://localhost).
            // En producción, usar un certificado válido y eliminar esta línea.
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.Add("X-Client-App", AppConfig.ClientAppName);
            return client;
        }

        private void AplicarToken()
        {
            _http.DefaultRequestHeaders.Authorization = SessionManager.EstaAutenticado
                ? new AuthenticationHeaderValue("Bearer", SessionManager.Token)
                : null;
        }

        public async Task<T?> GetAsync<T>(string ruta)
        {
            AplicarToken();
            var response = await _http.GetAsync(ruta);
            await LanzarSiHayError(response);
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string ruta, object body)
        {
            AplicarToken();
            var response = await _http.PostAsync(ruta, ContenidoJson(body));
            await LanzarSiHayError(response);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return default;
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task PostAsync(string ruta, object body)
        {
            AplicarToken();
            var response = await _http.PostAsync(ruta, ContenidoJson(body));
            await LanzarSiHayError(response);
        }

        public async Task PutAsync(string ruta, object body)
        {
            AplicarToken();
            var response = await _http.PutAsync(ruta, ContenidoJson(body));
            await LanzarSiHayError(response);
        }

        public async Task DeleteAsync(string ruta)
        {
            AplicarToken();
            var response = await _http.DeleteAsync(ruta);
            await LanzarSiHayError(response);
        }

        /// <summary>Descarga un archivo binario (ej. certificado) desde la API.</summary>
        public async Task<byte[]> DescargarArchivoAsync(string ruta)
        {
            AplicarToken();
            var response = await _http.GetAsync(ruta);
            await LanzarSiHayError(response);
            return await response.Content.ReadAsByteArrayAsync();
        }

        private static StringContent ContenidoJson(object body) =>
            new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private static async Task LanzarSiHayError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string mensaje = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Sesión inválida o vencida. Vuelve a iniciar sesión.",
                System.Net.HttpStatusCode.Forbidden => "No tienes permisos para realizar esta acción.",
                System.Net.HttpStatusCode.NotFound => "El recurso solicitado no existe.",
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
