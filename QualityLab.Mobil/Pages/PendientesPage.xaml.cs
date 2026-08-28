using QualityLab.Mobil.Services;

namespace QualityLab.Mobil.Pages
{
    public partial class PendientesPage : ContentPage
    {
        private readonly LocalQueueService _cola;
        private readonly SyncService _sync;

        public PendientesPage(LocalQueueService cola, SyncService sync)
        {
            InitializeComponent();
            _cola = cola;
            _sync = sync;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarPendientesAsync();
        }

        private async Task CargarPendientesAsync()
        {
            var pendientes = await _cola.ObtenerPendientesAsync();
            ListaPendientes.ItemsSource = pendientes;
        }

        private async void BtnSincronizar_Clicked(object? sender, EventArgs e)
        {
            BtnSincronizar.IsEnabled = false;
            BtnSincronizar.Text = "Sincronizando...";

            try
            {
                var resultado = await _sync.SincronizarPendientesAsync();

                if (resultado.TotalPendientes == 0)
                {
                    await DisplayAlert("Sincronización", "No había operaciones pendientes.", "OK");
                }
                else if (resultado.Sincronizados == resultado.TotalPendientes)
                {
                    await DisplayAlert("Sincronización completa",
                        $"Se enviaron correctamente {resultado.Sincronizados} operación(es).", "OK");
                }
                else
                {
                    await DisplayAlert("Sincronización parcial",
                        $"Se enviaron {resultado.Sincronizados} de {resultado.TotalPendientes}. " +
                        $"{resultado.ConError} quedaron pendientes (sin conexión con el servidor).", "OK");
                }
            }
            finally
            {
                BtnSincronizar.IsEnabled = true;
                BtnSincronizar.Text = "Sincronizar ahora";
                await CargarPendientesAsync();
            }
        }
    }
}