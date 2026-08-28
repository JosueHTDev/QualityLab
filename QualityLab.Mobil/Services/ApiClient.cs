using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using QualityLab.Mobil.Models;

namespace QualityLab.Mobil.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly SessionService _session;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(SessionService session)
        {
            _session = session;

            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
            _http.DefaultRequestHeaders.Add("X-Client-App", AppConfig.ClientAppName);
        }

        private void AplicarToken()
        {
            _http.DefaultRequestHeaders.Authorization = _session.EstaAutenticado
                ? new AuthenticationHeaderValue("Bearer", _session.Token)
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
            if (response.StatusCode == HttpStatusCode.NoContent) return default;
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<bool> HayConexionConApiAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                var response = await _http.GetAsync("api/health", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static StringContent ContenidoJson(object body) =>
            new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private static async Task LanzarSiHayError(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string mensaje = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión no es válida o venció. Vuelve a iniciar sesión.",
                HttpStatusCode.Forbidden => "No tienes permiso para realizar esta acción.",
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
            }

            throw new ApiException((int)response.StatusCode, mensaje);
        }
    }
}