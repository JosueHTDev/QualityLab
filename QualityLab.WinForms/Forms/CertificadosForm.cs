using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class CertificadosForm : Form
    {
        private readonly ApiClient _api = new();

        private readonly ComboBox _cmbMuestra = new() { Left = 130, Top = 20, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnConsultar = new() { Text = "Consultar estado", Left = 450, Top = 19, Width = 130 };

        private readonly Label _lblEstado = new() { Left = 20, Top = 70, Width = 600, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

        private readonly Button _btnEmitir = new() { Text = "Emitir certificado", Left = 20, Top = 110, Width = 160 };
        private readonly Button _btnDescargar = new() { Text = "Descargar certificado", Left = 200, Top = 110, Width = 160, Enabled = false };

        private List<MuestraResponse> _muestras = new();

        public CertificadosForm()
        {
            Text = "Certificados";
            Width = 650;
            Height = 250;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "Muestra:", Left = 20, Top = 23, Width = 100 });
            Controls.Add(_cmbMuestra);
            Controls.Add(_btnConsultar);
            Controls.Add(_lblEstado);
            Controls.Add(_btnEmitir);
            Controls.Add(_btnDescargar);

            // Solo ADMIN/SUPERVISOR pueden emitir certificados; el técnico y el cliente solo consultan/descargan.
            _btnEmitir.Visible = SessionManager.TieneRol("ADMIN", "SUPERVISOR");

            _btnConsultar.Click += async (_, _) => await ConsultarEstadoAsync();
            _btnEmitir.Click += BtnEmitir_Click;
            _btnDescargar.Click += BtnDescargar_Click;
            Load += async (_, _) => await CargarMuestrasAsync();
        }

        private async Task CargarMuestrasAsync()
        {
            try
            {
                _muestras = await _api.GetAsync<List<MuestraResponse>>("api/muestras") ?? new();
                _cmbMuestra.DataSource = _muestras;
                _cmbMuestra.DisplayMember = "Codigo";
                _cmbMuestra.ValueMember = "Id";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ConsultarEstadoAsync()
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;

            try
            {
                var estado = await _api.GetAsync<EstadoMuestraDto>($"api/muestras/{muestraId}/estado");
                if (estado is null) return;

                _lblEstado.Text = $"Estado: {estado.Estado}   |   Certificado emitido: {(estado.TieneCertificado ? "Sí" : "No")}";
                _btnDescargar.Enabled = estado.TieneCertificado;
                _btnEmitir.Enabled = !estado.TieneCertificado && estado.Estado == "Analizada";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEmitir_Click(object? sender, EventArgs e)
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;

            try
            {
                await _api.PostAsync<CertificadoResponse>($"api/certificados/muestra/{muestraId}/emitir", new { });
                MessageBox.Show("Certificado emitido correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ConsultarEstadoAsync();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDescargar_Click(object? sender, EventArgs e)
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;
            var muestra = _muestras.First(m => m.Id == muestraId);

            try
            {
                var contenido = await _api.DescargarArchivoAsync($"api/certificados/muestra/{muestraId}/descargar");

                using var dialogo = new SaveFileDialog
                {
                    FileName = $"Certificado_{muestra.Codigo}.txt",
                    Filter = "Archivo de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*"
                };

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(dialogo.FileName, contenido);
                    MessageBox.Show("Certificado descargado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}