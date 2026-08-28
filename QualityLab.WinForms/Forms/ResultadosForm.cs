using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class ResultadosForm : Form
    {
        private readonly ApiClient _api = new();

        private readonly ComboBox _cmbMuestra = new() { Left = 130, Top = 15, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cmbAnalisis = new() { Left = 440, Top = 15, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnCargar = new() { Text = "Cargar", Left = 720, Top = 14, Width = 100 };

        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        // Panel para registrar un nuevo resultado (ADMIN/SUPERVISOR/TECNICO)
        private readonly Panel _panelCrear = new() { Dock = DockStyle.Bottom, Height = 150 };
        private readonly TextBox _txtParametro = new() { Left = 150, Top = 15, Width = 220 };
        private readonly TextBox _txtValorObtenido = new() { Left = 150, Top = 45, Width = 150 };
        private readonly TextBox _txtValorReferencia = new() { Left = 150, Top = 75, Width = 150 };
        private readonly TextBox _txtUnidad = new() { Left = 150, Top = 105, Width = 100 };
        private readonly CheckBox _chkConforme = new() { Text = "Conforme", Left = 420, Top = 47, Width = 120 };
        private readonly Button _btnRegistrar = new() { Text = "Registrar resultado", Left = 420, Top = 100, Width = 150 };

        private List<MuestraResponse> _muestras = new();
        private List<AnalisisResponse> _analisisDeLaMuestra = new();

        public ResultadosForm()
        {
            Text = "Resultados de análisis";
            Width = 950;
            Height = 620;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            panelTop.Controls.Add(new Label { Text = "Muestra:", Left = 20, Top = 18, Width = 100 });
            panelTop.Controls.Add(_cmbMuestra);
            panelTop.Controls.Add(new Label { Text = "Análisis:", Left = 380, Top = 18, Width = 60 });
            panelTop.Controls.Add(_cmbAnalisis);
            panelTop.Controls.Add(_btnCargar);

            ConstruirPanelCrear();

            Controls.Add(_grid);
            Controls.Add(_panelCrear);
            Controls.Add(panelTop);

            _cmbMuestra.SelectedIndexChanged += async (_, _) => await CargarAnalisisDeLaMuestraAsync();
            _btnCargar.Click += async (_, _) => await CargarResultadosAsync();
            Load += async (_, _) => await CargarMuestrasAsync();
        }

        private void ConstruirPanelCrear()
        {
            _panelCrear.Controls.Add(new Label { Text = "Parámetro:", Left = 20, Top = 18, Width = 120 });
            _panelCrear.Controls.Add(new Label { Text = "Valor obtenido:", Left = 20, Top = 48, Width = 120 });
            _panelCrear.Controls.Add(new Label { Text = "Valor de referencia:", Left = 20, Top = 78, Width = 120 });
            _panelCrear.Controls.Add(new Label { Text = "Unidad:", Left = 20, Top = 108, Width = 120 });
            _panelCrear.Controls.Add(_txtParametro);
            _panelCrear.Controls.Add(_txtValorObtenido);
            _panelCrear.Controls.Add(_txtValorReferencia);
            _panelCrear.Controls.Add(_txtUnidad);
            _panelCrear.Controls.Add(_chkConforme);
            _panelCrear.Controls.Add(_btnRegistrar);
            _btnRegistrar.Click += BtnRegistrar_Click;

            // Solo el staff del laboratorio registra resultados (no CLIENTE, que solo consulta)
            _panelCrear.Visible = SessionManager.TieneRol("ADMIN", "SUPERVISOR", "TECNICO");
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

        private async Task CargarAnalisisDeLaMuestraAsync()
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;

            try
            {
                _analisisDeLaMuestra = await _api.GetAsync<List<AnalisisResponse>>($"api/analisis/muestra/{muestraId}") ?? new();
                _cmbAnalisis.DataSource = _analisisDeLaMuestra;
                _cmbAnalisis.DisplayMember = "TipoAnalisis";
                _cmbAnalisis.ValueMember = "Id";

                await CargarResultadosAsync();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarResultadosAsync()
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;

            try
            {
                // El acceso a resultados es restringido: la API valida que el usuario
                // (cliente dueño / técnico asignado / staff) tenga permiso sobre la muestra.
                var resultados = await _api.GetAsync<List<ResultadoResponse>>($"api/resultados/muestra/{muestraId}");
                _grid.DataSource = resultados;
                if (_grid.Columns.Count > 0)
                {
                    _grid.Columns["AnalisisId"].Visible = false;
                    _grid.Columns["MuestraId"].Visible = false;
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (_cmbAnalisis.SelectedValue is not int analisisId || string.IsNullOrWhiteSpace(_txtParametro.Text))
            {
                MessageBox.Show("Selecciona un análisis e indica el parámetro.", "Datos incompletos");
                return;
            }

            try
            {
                await _api.PostAsync<ResultadoResponse>("api/resultados", new ResultadoCreateDto
                {
                    AnalisisId = analisisId,
                    Parametro = _txtParametro.Text.Trim(),
                    ValorObtenido = _txtValorObtenido.Text.Trim(),
                    ValorReferencia = _txtValorReferencia.Text.Trim(),
                    Unidad = _txtUnidad.Text.Trim(),
                    Conforme = _chkConforme.Checked
                });

                LimpiarFormulario();
                await CargarResultadosAsync();
                MessageBox.Show("Resultado registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            _txtParametro.Clear();
            _txtValorObtenido.Clear();
            _txtValorReferencia.Clear();
            _txtUnidad.Clear();
            _chkConforme.Checked = false;
        }
    }
}