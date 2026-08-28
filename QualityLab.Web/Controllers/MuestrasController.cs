using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualityLab.Web.Models;
using QualityLab.Web.Services;

namespace QualityLab.Web.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class MuestrasController : Controller
    {
        private readonly ApiClient _api;

        public MuestrasController(ApiClient api)
        {
            _api = api;
        }

        /// <summary>"Consultar muestra": lista de todas las muestras del cliente autenticado.</summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // La API ya filtra por el clienteId embebido en el JWT.
                var muestras = await _api.GetAsync<List<MuestraResponse>>("api/muestras");
                return View(muestras ?? new List<MuestraResponse>());
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<MuestraResponse>());
            }
        }

        /// <summary>"Consultar estado" + "Consultar resultado" de una muestra puntual.</summary>
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var muestra = await _api.GetAsync<MuestraResponse>($"api/muestras/{id}");
                var estado = await _api.GetAsync<EstadoMuestraResponse>($"api/muestras/{id}/estado");

                var resultados = new List<ResultadoResponse>();

                // El resultado solo se muestra si el análisis ya terminó
                // (la API igual protege el acceso, esto solo evita una llamada innecesaria).
                if (estado is not null &&
                    (estado.Estado == "Analizada" || estado.Estado == "CertificadoEmitido"))
                {
                    resultados = await _api.GetAsync<List<ResultadoResponse>>($"api/resultados/muestra/{id}")
                                 ?? new List<ResultadoResponse>();
                }

                return View(new MuestraDetalleViewModel
                {
                    Muestra = muestra ?? new MuestraResponse(),
                    Estado = estado ?? new EstadoMuestraResponse(),
                    Resultados = resultados
                });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>"Descargar certificado".</summary>
        public async Task<IActionResult> DescargarCertificado(int id)
        {
            try
            {
                var (contenido, nombreArchivo, contentType) =
                    await _api.DescargarArchivoAsync($"api/certificados/muestra/{id}/descargar");

                return File(contenido, contentType, nombreArchivo);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Detalle), new { id });
            }
        }
    }
}
