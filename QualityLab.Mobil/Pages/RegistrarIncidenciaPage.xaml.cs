using QualityLab.Mobil.Models;
using QualityLab.Mobil.Services;

namespace QualityLab.Mobil.Pages
{
    [QueryProperty(nameof(MuestraId), "MuestraId")]
    [QueryProperty(nameof(Codigo), "Codigo")]
    public partial class RegistrarIncidenciaPage : ContentPage
    {
        private readonly ApiClient _api;
        private readonly LocalQueueService _cola;

        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;

        public RegistrarIncidenciaPage(ApiClient api, LocalQueueService cola)
        {
            InitializeComponent();
            _api = api;
            _cola = cola;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LabelMuestra.Text = $"Muestra: {Codigo}";
        }

        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditorDescripcion.Text))
            {
                await DisplayAlert("Datos incompletos", "Describe la incidencia encontrada.", "OK");
                return;
            }

            var descripcion = EditorDescripcion.Text.Trim();

            var operacion = await _cola.EncolarIncidenciaAsync(MuestraId, descripcion);

            LabelEstadoEnvio.IsVisible = true;
            LabelEstadoEnvio.Text = "Guardado localmente. Intentando enviar al servidor...";

            try
            {
                await _api.PostAsync<object>("api/tecnico/incidencias", new RegistrarIncidenciaDto
                {
                    MuestraId = MuestraId,
                    Descripcion = descripcion,
                    IdLocalOrigen = operacion.IdLocalOrigen,
                    FechaCreacionLocal = operacion.FechaCreacionLocal
                });

                await _cola.MarcarSincronizadaAsync(operacion);
                await DisplayAlert("Éxito", "Incidencia registrada y enviada al servidor.", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Guardado sin conexión",
                    "No se pudo contactar al servidor en este momento. La incidencia quedó guardada en el " +
                    "dispositivo y se enviará automáticamente la próxima vez que haya conexión.",
                    "Entendido");
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}