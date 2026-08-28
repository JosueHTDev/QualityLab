using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class AnalisisForm : Form
    {
        private readonly ApiClient _api = new();

        private readonly ComboBox _cmbMuestra = new() { Left = 130, Top = 15, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnCargar = new() { Text = "Cargar análisis", Left = 450, Top = 14, Width = 130 };

        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        // Panel crear (solo ADMIN/SUPERVISOR)
        private readonly Panel _panelCrear = new() { Dock = DockStyle.Top, Height = 60 };
        private readonly ComboBox _cmbTecnico = new() { Left = 200, Top = 15, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtTipoAnalisis = new() { Left = 440, Top = 15, Width = 220 };
        private readonly Button _btnCrear = new() { Text = "Crear análisis", Left = 680, Top = 14, Width = 120 };

        // Panel completar (todos los roles del laboratorio, la API valida propiedad)
        private readonly Panel _panelCompletar = new() { Dock = DockStyle.Bottom, Height = 70 };
        private readonly TextBox _txtObservaciones = new() { Left = 130, Top = 15, Width = 500 };
        private readonly Button _btnCompletar = new() { Text = "Marcar análisis seleccionado como Completado", Left = 130, Top = 40, Width = 320 };

        private List<MuestraResponse> _muestras = new();

        public AnalisisForm()
        {
            Text = "Análisis de laboratorio";
            Width = 950;
            Height = 600;

            var panelTop = new Panel { Dock = DockStyle.Top, Height = 50 };
            panelTop.Controls.Add(new Label { Text = "Muestra:", Left = 20, Top = 18, Width = 100 });
            panelTop.Controls.Add(_cmbMuestra);
            panelTop.Controls.Add(_btnCargar);

            ConstruirPanelCrear();
            ConstruirPanelCompletar();

            Controls.Add(_grid);
            Controls.Add(_panelCompletar);
            Controls.Add(_panelCrear);
            Controls.Add(panelTop);

            _btnCargar.Click += async (_, _) => await CargarAnalisisAsync();
            Load += async (_, _) => await CargarDatosIniciales();
        }

        private void ConstruirPanelCrear()
        {
            _panelCrear.Controls.Add(new Label { Text = "Técnico:", Left = 20, Top = 18, Width = 90 });
            _panelCrear.Controls.Add(_cmbTecnico);
            _panelCrear.Controls.Add(new Label { Text = "Tipo de análisis:", Left = 350, Top = 18, Width = 100 });
            _panelCrear.Controls.Add(_txtTipoAnalisis);
            _panelCrear.Controls.Add(_btnCrear);
            _btnCrear.Click += BtnCrear_Click;

            _panelCrear.Visible = SessionManager.TieneRol("ADMIN", "SUPERVISOR");
        }

        private void ConstruirPanelCompletar()
        {
            _panelCompletar.Controls.Add(new Label { Text = "Observaciones:", Left = 20, Top = 18, Width = 100 });
            _panelCompletar.Controls.Add(_txtObservaciones);
            _panelCompletar.Controls.Add(_btnCompletar);
            _btnCompletar.Click += BtnCompletar_Click;
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                // La API ya filtra: TECNICO solo ve sus muestras asignadas.
                _muestras = await _api.GetAsync<List<MuestraResponse>>("api/muestras") ?? new();
                _cmbMuestra.DataSource = _muestras;
                _cmbMuestra.DisplayMember = "Codigo";
                _cmbMuestra.ValueMember = "Id";

                if (SessionManager.TieneRol("ADMIN", "SUPERVISOR"))
                {
                    var tecnicos = await _api.GetAsync<List<TecnicoResponse>>("api/tecnicos");
                    _cmbTecnico.DataSource = tecnicos;
                    _cmbTecnico.DisplayMember = "Nombres";
                    _cmbTecnico.ValueMember = "Id";
                }

                if (_muestras.Any()) await CargarAnalisisAsync();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarAnalisisAsync()
        {
            if (_cmbMuestra.SelectedValue is not int muestraId) return;

            try
            {
                var analisis = await _api.GetAsync<List<AnalisisResponse>>($"api/analisis/muestra/{muestraId}");
                _grid.DataSource = analisis;
                if (_grid.Columns.Count > 0)
                    _grid.Columns["MuestraId"].Visible = false;
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCrear_Click(object? sender, EventArgs e)
        {
            if (_cmbMuestra.SelectedValue is not int muestraId || _cmbTecnico.SelectedValue is not int tecnicoId
                || string.IsNullOrWhiteSpace(_txtTipoAnalisis.Text))
            {
                MessageBox.Show("Selecciona muestra, técnico e indica el tipo de análisis.", "Datos incompletos");
                return;
            }

            try
            {
                await _api.PostAsync<AnalisisResponse>("api/analisis", new AnalisisCreateDto
                {
                    MuestraId = muestraId,
                    TecnicoId = tecnicoId,
                    TipoAnalisis = _txtTipoAnalisis.Text.Trim()
                });

                _txtTipoAnalisis.Clear();
                await CargarAnalisisAsync();
                MessageBox.Show("Análisis creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCompletar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not AnalisisResponse analisis)
            {
                MessageBox.Show("Selecciona un análisis de la lista.", "Falta selección");
                return;
            }

            try
            {
                await _api.PutAsync($"api/analisis/{analisis.Id}/completar", new CompletarAnalisisDto
                {
                    Observaciones = _txtObservaciones.Text.Trim()
                });

                _txtObservaciones.Clear();
                await CargarAnalisisAsync();
                MessageBox.Show("Análisis marcado como completado. Ya puedes registrar el resultado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}