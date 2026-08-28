using QualityLab.Mobil.Models;
using QualityLab.Mobil.Services;

namespace QualityLab.Mobil.Pages
{
    [QueryProperty(nameof(MuestraId), "MuestraId")]
    [QueryProperty(nameof(Codigo), "Codigo")]
    public partial class RegistrarAvancePage : ContentPage
    {
        private readonly ApiClient _api;
        private readonly LocalQueueService _cola;

        public int MuestraId { get; set; }
        public string Codigo { get; set; } = string.Empty;

        public RegistrarAvancePage(ApiClient api, LocalQueueService cola)
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

        private void SliderPorcentaje_ValueChanged(object? sender, ValueChangedEventArgs e)
        {
            LabelPorcentaje.Text = $"{(int)e.NewValue}%";
        }

        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditorDescripcion.Text))
            {
                await DisplayAlert("Datos incompletos", "Describe el avance realizado.", "OK");
                return;
            }

            var descripcion = EditorDescripcion.Text.Trim();
            var porcentaje = (int)SliderPorcentaje.Value;

            var operacion = await _cola.EncolarAvanceAsync(MuestraId, descripcion, porcentaje);

            LabelEstadoEnvio.IsVisible = true;
            LabelEstadoEnvio.Text = "Guardado localmente. Intentando enviar al servidor...";

            try
            {
                await _api.PostAsync<object>("api/tecnico/avances", new RegistrarAvanceDto
                {
                    MuestraId = MuestraId,
                    Descripcion = descripcion,
                    PorcentajeAvance = porcentaje,
                    IdLocalOrigen = operacion.IdLocalOrigen,
                    FechaCreacionLocal = operacion.FechaCreacionLocal
                });

                await _cola.MarcarSincronizadaAsync(operacion);
                await DisplayAlert("Éxito", "Avance registrado y enviado al servidor.", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Guardado sin conexión",
                    "No se pudo contactar al servidor en este momento. El avance quedó guardado en el " +
                    "dispositivo y se enviará automáticamente la próxima vez que haya conexión.",
                    "Entendido");
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}