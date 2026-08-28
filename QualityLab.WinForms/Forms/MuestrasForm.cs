using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class MuestrasForm : Form
    {
        private readonly ApiClient _api = new();
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        // --- Panel de registro (solo ADMIN/SUPERVISOR) ---
        private readonly Panel _panelCrear = new() { Dock = DockStyle.Top, Height = 120 };
        private readonly TextBox _txtCodigo = new() { Left = 130, Top = 15, Width = 180 };
        private readonly ComboBox _cmbLote = new() { Left = 400, Top = 15, Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtTipoProducto = new() { Left = 130, Top = 45, Width = 350 };
        private readonly Button _btnRegistrar = new() { Text = "Registrar muestra", Left = 130, Top = 80, Width = 140 };

        // --- Panel de asignación de técnico (solo ADMIN/SUPERVISOR) ---
        private readonly Panel _panelAsignar = new() { Dock = DockStyle.Top, Height = 60 };
        private readonly ComboBox _cmbTecnico = new() { Left = 130, Top = 15, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnAsignar = new() { Text = "Asignar técnico a la muestra seleccionada", Left = 400, Top = 14, Width = 260 };

        private readonly Button _btnRefrescar = new() { Text = "Refrescar lista", Dock = DockStyle.Bottom, Height = 30 };

        private List<LoteResponse> _lotes = new();

        public MuestrasForm()
        {
            Text = "Muestras - Registro y asignación de técnico";
            Width = 950;
            Height = 600;

            ConstruirPanelCrear();
            ConstruirPanelAsignar();

            Controls.Add(_grid);
            Controls.Add(_btnRefrescar);
            Controls.Add(_panelAsignar);
            Controls.Add(_panelCrear);

            _btnRefrescar.Click += async (_, _) => await CargarMuestrasAsync();
            Load += async (_, _) => await CargarDatosIniciales();
        }

        private void ConstruirPanelCrear()
        {
            _panelCrear.Controls.Add(new Label { Text = "Código muestra:", Left = 20, Top = 18, Width = 105 });
            _panelCrear.Controls.Add(new Label { Text = "Lote:", Left = 360, Top = 18, Width = 40 });
            _panelCrear.Controls.Add(new Label { Text = "Tipo de producto:", Left = 20, Top = 48, Width = 105 });
            _panelCrear.Controls.Add(_txtCodigo);
            _panelCrear.Controls.Add(_cmbLote);
            _panelCrear.Controls.Add(_txtTipoProducto);
            _panelCrear.Controls.Add(_btnRegistrar);
            _btnRegistrar.Click += BtnRegistrar_Click;

            // Solo el staff que puede crear muestras ve este panel
            _panelCrear.Visible = SessionManager.TieneRol("ADMIN", "SUPERVISOR");
        }

        private void ConstruirPanelAsignar()
        {
            _panelAsignar.Controls.Add(new Label { Text = "Técnico:", Left = 20, Top = 18, Width = 100 });
            _panelAsignar.Controls.Add(_cmbTecnico);
            _panelAsignar.Controls.Add(_btnAsignar);
            _btnAsignar.Click += BtnAsignar_Click;

            _panelAsignar.Visible = SessionManager.TieneRol("ADMIN", "SUPERVISOR");
        }

        private async Task CargarDatosIniciales()
        {
            if (SessionManager.TieneRol("ADMIN", "SUPERVISOR"))
            {
                await CargarLotesAsync();
                await CargarTecnicosAsync();
            }
            await CargarMuestrasAsync();
        }

        private async Task CargarLotesAsync()
        {
            try
            {
                _lotes = await _api.GetAsync<List<LoteResponse>>("api/lotes") ?? new();
                _cmbLote.DataSource = _lotes;
                _cmbLote.DisplayMember = "Codigo";
                _cmbLote.ValueMember = "Id";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarTecnicosAsync()
        {
            try
            {
                var tecnicos = await _api.GetAsync<List<TecnicoResponse>>("api/tecnicos");
                _cmbTecnico.DataSource = tecnicos;
                _cmbTecnico.DisplayMember = "Nombres";
                _cmbTecnico.ValueMember = "Id";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarMuestrasAsync()
        {
            try
            {
                // La API filtra automáticamente según el rol:
                // ADMIN/SUPERVISOR ven todas, TECNICO solo las asignadas a él.
                var muestras = await _api.GetAsync<List<MuestraResponse>>("api/muestras");
                _grid.DataSource = muestras;
                if (_grid.Columns.Count > 0)
                {
                    _grid.Columns["Id"].Visible = false;
                    _grid.Columns["LoteId"].Visible = false;
                    _grid.Columns["ClienteId"].Visible = false;
                    _grid.Columns["TecnicoAsignadoId"].Visible = false;
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCodigo.Text) || _cmbLote.SelectedValue is null)
            {
                MessageBox.Show("Código y lote son obligatorios.", "Datos incompletos");
                return;
            }

            var loteSeleccionado = _lotes.First(l => l.Id == (int)_cmbLote.SelectedValue);

            try
            {
                await _api.PostAsync<MuestraResponse>("api/muestras", new MuestraCreateDto
                {
                    Codigo = _txtCodigo.Text.Trim(),
                    LoteId = loteSeleccionado.Id,
                    ClienteId = loteSeleccionado.ClienteId,
                    TipoProducto = _txtTipoProducto.Text.Trim()
                });

                _txtCodigo.Clear();
                _txtTipoProducto.Clear();
                await CargarMuestrasAsync();
                MessageBox.Show("Muestra registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAsignar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not MuestraResponse muestra)
            {
                MessageBox.Show("Selecciona una muestra de la lista.", "Falta selección");
                return;
            }

            if (_cmbTecnico.SelectedValue is null)
            {
                MessageBox.Show("Selecciona un técnico.", "Falta selección");
                return;
            }

            try
            {
                await _api.PostAsync($"api/muestras/{muestra.Id}/asignar-tecnico", new AsignarTecnicoDto
                {
                    TecnicoId = (int)_cmbTecnico.SelectedValue
                });

                await CargarMuestrasAsync();
                MessageBox.Show("Técnico asignado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
