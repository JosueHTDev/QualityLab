using QualityLab.Mobil.Models;
using SQLite;

namespace QualityLab.Mobil.Services
{
    public class LocalQueueService
    {
        private SQLiteAsyncConnection? _db;

        private async Task<SQLiteAsyncConnection> ObtenerConexionAsync()
        {
            if (_db is not null) return _db;

            var rutaBd = Path.Combine(FileSystem.AppDataDirectory, "qualitylab_local.db3");
            _db = new SQLiteAsyncConnection(rutaBd);
            await _db.CreateTableAsync<OperacionPendiente>();
            return _db;
        }

        public async Task<OperacionPendiente> EncolarAvanceAsync(int muestraId, string descripcion, int porcentaje)
        {
            var operacion = new OperacionPendiente
            {
                IdLocalOrigen = Guid.NewGuid(),
                Tipo = TipoOperacion.Avance,
                MuestraId = muestraId,
                Descripcion = descripcion,
                PorcentajeAvance = porcentaje,
                FechaCreacionLocal = DateTime.UtcNow
            };

            var db = await ObtenerConexionAsync();
            await db.InsertAsync(operacion);
            return operacion;
        }

        public async Task<OperacionPendiente> EncolarIncidenciaAsync(int muestraId, string descripcion)
        {
            var operacion = new OperacionPendiente
            {
                IdLocalOrigen = Guid.NewGuid(),
                Tipo = TipoOperacion.Incidencia,
                MuestraId = muestraId,
                Descripcion = descripcion,
                FechaCreacionLocal = DateTime.UtcNow
            };

            var db = await ObtenerConexionAsync();
            await db.InsertAsync(operacion);
            return operacion;
        }

        public async Task<List<OperacionPendiente>> ObtenerPendientesAsync()
        {
            var db = await ObtenerConexionAsync();
            return await db.Table<OperacionPendiente>()
                .Where(o => !o.Sincronizado)
                .OrderBy(o => o.FechaCreacionLocal)
                .ToListAsync();
        }

        public async Task MarcarSincronizadaAsync(OperacionPendiente operacion)
        {
            operacion.Sincronizado = true;
            operacion.ErrorUltimoIntento = null;
            var db = await ObtenerConexionAsync();
            await db.UpdateAsync(operacion);
        }

        public async Task GuardarErrorAsync(OperacionPendiente operacion, string error)
        {
            operacion.ErrorUltimoIntento = error;
            var db = await ObtenerConexionAsync();
            await db.UpdateAsync(operacion);
        }

        public async Task<int> ContarPendientesAsync()
        {
            var db = await ObtenerConexionAsync();
            return await db.Table<OperacionPendiente>().Where(o => !o.Sincronizado).CountAsync();
        }
    }
}