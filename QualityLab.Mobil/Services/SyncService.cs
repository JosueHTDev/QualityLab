using QualityLab.Mobil.Models;

namespace QualityLab.Mobil.Services
{
    public class ResultadoSincronizacion
    {
        public int TotalPendientes { get; set; }
        public int Sincronizados { get; set; }
        public int ConError { get; set; }
    }

    public class SyncService
    {
        private readonly ApiClient _api;
        private readonly LocalQueueService _cola;

        public SyncService(ApiClient api, LocalQueueService cola)
        {
            _api = api;
            _cola = cola;
        }

        public async Task<ResultadoSincronizacion> SincronizarPendientesAsync()
        {
            var resultado = new ResultadoSincronizacion();
            var pendientes = await _cola.ObtenerPendientesAsync();
            resultado.TotalPendientes = pendientes.Count;

            if (pendientes.Count == 0) return resultado;

            if (!await _api.HayConexionConApiAsync())
            {
                resultado.ConError = pendientes.Count;
                return resultado;
            }

            foreach (var operacion in pendientes)
            {
                try
                {
                    if (operacion.Tipo == TipoOperacion.Avance)
                    {
                        await _api.PostAsync<object>("api/tecnico/avances", new RegistrarAvanceDto
                        {
                            MuestraId = operacion.MuestraId,
                            Descripcion = operacion.Descripcion,
                            PorcentajeAvance = operacion.PorcentajeAvance,
                            IdLocalOrigen = operacion.IdLocalOrigen,
                            FechaCreacionLocal = operacion.FechaCreacionLocal
                        });
                    }
                    else
                    {
                        await _api.PostAsync<object>("api/tecnico/incidencias", new RegistrarIncidenciaDto
                        {
                            MuestraId = operacion.MuestraId,
                            Descripcion = operacion.Descripcion,
                            IdLocalOrigen = operacion.IdLocalOrigen,
                            FechaCreacionLocal = operacion.FechaCreacionLocal
                        });
                    }

                    await _cola.MarcarSincronizadaAsync(operacion);
                    resultado.Sincronizados++;
                }
                catch (Exception ex)
                {
                    await _cola.GuardarErrorAsync(operacion, ex.Message);
                    resultado.ConError++;
                }
            }

            return resultado;
        }
    }
}