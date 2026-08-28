using QualityLab.Mobil.Models;
using QualityLab.Mobil.Services;

namespace QualityLab.Mobil.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiClient _api;
        private readonly SessionService _session;

        public LoginPage(ApiClient api, SessionService session)
        {
            InitializeComponent();
            _api = api;
            _session = session;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_session.EstaAutenticado)
            {
                await Shell.Current.GoToAsync(nameof(MuestrasAsignadasPage));
            }
        }

        private async void BtnIngresar_Clicked(object? sender, EventArgs e)
        {
            LabelError.IsVisible = false;

            if (string.IsNullOrWhiteSpace(EntryUsuario.Text) || string.IsNullOrWhiteSpace(EntryPassword.Text))
            {
                MostrarError("Ingresa usuario y contraseña.");
                return;
            }

            BtnIngresar.IsEnabled = false;
            Indicador.IsRunning = true;
            Indicador.IsVisible = true;

            try
            {
                var respuesta = await _api.PostAsync<LoginResponse>("api/auth/login", new LoginRequest
                {
                    NombreUsuario = EntryUsuario.Text.Trim(),
                    Password = EntryPassword.Text
                });

                if (respuesta is null)
                {
                    MostrarError("No se pudo iniciar sesión.");
                    return;
                }

                if (!string.Equals(respuesta.Rol, "TECNICO", StringComparison.OrdinalIgnoreCase))
                {
                    MostrarError("Esta app es exclusiva para técnicos del laboratorio.");
                    return;
                }

                _session.IniciarSesion(respuesta);
                EntryPassword.Text = string.Empty;

                await Shell.Current.GoToAsync(nameof(MuestrasAsignadasPage));
            }
            catch (ApiException ex)
            {
                MostrarError(ex.Message);
            }
            catch (HttpRequestException)
            {
                MostrarError($"No se pudo conectar con la API en {AppConfig.BaseUrl}. Verifica que esté en ejecución y que la IP configurada sea correcta.");
            }
            catch (TaskCanceledException)
            {
                MostrarError("El servidor tardó demasiado en responder.");
            }
            finally
            {
                BtnIngresar.IsEnabled = true;
                Indicador.IsRunning = false;
                Indicador.IsVisible = false;
            }
        }

        private void MostrarError(string mensaje)
        {
            LabelError.Text = mensaje;
            LabelError.IsVisible = true;
        }
    }
}