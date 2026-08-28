using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class LoginForm : Form
    {
        private readonly TextBox _txtUsuario = new() { Left = 140, Top = 40, Width = 200 };
        private readonly TextBox _txtPassword = new() { Left = 140, Top = 80, Width = 200, UseSystemPasswordChar = true };
        private readonly Button _btnIngresar = new() { Text = "Ingresar", Left = 140, Top = 120, Width = 100 };
        private readonly Label _lblEstado = new() { Left = 20, Top = 160, Width = 340, ForeColor = Color.Firebrick, Text = "" };
        private readonly ApiClient _api = new();

        public LoginForm()
        {
            Text = "QualityLab - Iniciar sesión";
            Width = 400;
            Height = 260;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(new Label { Text = "Usuario:", Left = 40, Top = 43, Width = 90 });
            Controls.Add(new Label { Text = "Contraseña:", Left = 40, Top = 83, Width = 90 });
            Controls.Add(_txtUsuario);
            Controls.Add(_txtPassword);
            Controls.Add(_btnIngresar);
            Controls.Add(_lblEstado);

            _btnIngresar.Click += BtnIngresar_Click;
            AcceptButton = _btnIngresar;
        }

        private async void BtnIngresar_Click(object? sender, EventArgs e)
        {
            _lblEstado.Text = "";

            if (string.IsNullOrWhiteSpace(_txtUsuario.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                _lblEstado.Text = "Ingresa usuario y contraseña.";
                return;
            }

            _btnIngresar.Enabled = false;
            _btnIngresar.Text = "Ingresando...";

            try
            {
                var respuesta = await _api.PostAsync<LoginResponse>("api/auth/login", new LoginRequest
                {
                    NombreUsuario = _txtUsuario.Text.Trim(),
                    Password = _txtPassword.Text
                });

                if (respuesta is null)
                {
                    _lblEstado.Text = "No se pudo iniciar sesión.";
                    return;
                }

                SessionManager.IniciarSesion(respuesta);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ApiException ex)
            {
                _lblEstado.Text = ex.Message;
            }
            catch (HttpRequestException)
            {
                _lblEstado.Text = $"No se pudo conectar con la API en {AppConfig.BaseUrl}. " +
                                   "Verifica que QualityLab.API esté en ejecución.";
            }
            finally
            {
                _btnIngresar.Enabled = true;
                _btnIngresar.Text = "Ingresar";
            }
        }
    }
}
