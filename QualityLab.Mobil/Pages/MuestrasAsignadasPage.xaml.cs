using QualityLab.Mobil.Models;
using QualityLab.Mobil.Services;

namespace QualityLab.Mobil.Pages
{
    public partial class MuestrasAsignadasPage : ContentPage
    {
        private readonly ApiClient _api;
        private readonly SessionService _session;
        private readonly LocalQueueService _cola;
        private readonly SyncService _sync;

        public MuestrasAsignadasPage(ApiClient api, SessionService session, LocalQueueService cola, SyncService sync)
        {
            InitializeComponent();
            _api = api;
            _session = session;
            _cola = cola;
            _sync = sync;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_session.EstaAutenticado)
            {
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                return;
            }

            LabelUsuario.Text = $"Técnico: {_session.NombreUsuario}";

            await IntentarSincronizarSilenciosoAsync();
            await ActualizarContadorPendientesAsync();
            await CargarMuestrasAsync();
        }

        private async void RefreshView_Refreshing(object? sender, EventArgs e)
        {
            await CargarMuestrasAsync();
            RefreshView.IsRefreshing = false;
        }

        private async void BtnRefrescar_Clicked(object? sender, EventArgs e) => await CargarMuestrasAsync();

        private async Task CargarMuestrasAsync()
        {
            try
            {
                var muestras = await _api.GetAsync<List<MuestraResponse>>("api/muestras/asignadas-a-mi");
                ListaMuestras.ItemsSource = muestras;
            }
            catch (ApiException ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Sin conexión",
                    "No se pudo conectar con el servidor. Puedes seguir registrando avances e incidencias: " +
                    "se guardarán en el dispositivo y se enviarán automáticamente cuando vuelva la conexión.",
                    "Entendido");
            }
        }

        private async void MuestraSeleccionada_Tapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not MuestraResponse muestra) return;

            await Shell.Current.GoToAsync(nameof(DetalleMuestraPage), new Dictionary<string, object>
            {
                { "MuestraId", muestra.Id },
                { "Codigo", muestra.Codigo },
                { "TipoProducto", muestra.TipoProducto },
                { "Estado", muestra.Estado }
            });
        }

        private async void BtnPendientes_Clicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PendientesPage));
        }

        private async void BtnCerrarSesion_Clicked(object? sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Cerrar sesión", "¿Seguro que deseas salir?", "Sí", "Cancelar");
            if (!confirmar) return;

            _session.CerrarSesion();
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        private async Task IntentarSincronizarSilenciosoAsync()
        {
            var pendientes = await _cola.ContarPendientesAsync();
            if (pendientes == 0) return;

            var resultado = await _sync.SincronizarPendientesAsync();
            if (resultado.Sincronizados > 0)
            {
                await DisplayAlert("Sincronización",
                    $"Se sincronizaron {resultado.Sincronizados} registro(s) pendientes con el servidor.",
                    "OK");
            }
        }

        private async Task ActualizarContadorPendientesAsync()
        {
            var pendientes = await _cola.ContarPendientesAsync();
            BtnPendientes.Text = $"Pendientes por sincronizar ({pendientes})";
        }
    }
}