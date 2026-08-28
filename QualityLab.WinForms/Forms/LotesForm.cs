using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class LotesForm : Form
    {
        private readonly ApiClient _api = new();
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        private readonly TextBox _txtCodigo = new() { Left = 130, Top = 20, Width = 200 };
        private readonly ComboBox _cmbCliente = new() { Left = 130, Top = 50, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtDescripcion = new() { Left = 130, Top = 80, Width = 350 };
        private readonly Button _btnGuardar = new() { Text = "Guardar", Left = 130, Top = 115, Width = 100 };
        private readonly Button _btnRefrescar = new() { Text = "Refrescar lista", Left = 240, Top = 115, Width = 120 };

        public LotesForm()
        {
            Text = "Gestión de Lotes";
            Width = 800;
            Height = 500;

            var panelForm = new Panel { Dock = DockStyle.Top, Height = 160 };
            panelForm.Controls.Add(new Label { Text = "Código:", Left = 20, Top = 23, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Cliente:", Left = 20, Top = 53, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Descripción:", Left = 20, Top = 83, Width = 100 });
            panelForm.Controls.Add(_txtCodigo);
            panelForm.Controls.Add(_cmbCliente);
            panelForm.Controls.Add(_txtDescripcion);
            panelForm.Controls.Add(_btnGuardar);
            panelForm.Controls.Add(_btnRefrescar);

            Controls.Add(_grid);
            Controls.Add(panelForm);

            _btnGuardar.Click += BtnGuardar_Click;
            _btnRefrescar.Click += async (_, _) => await CargarLotesAsync();
            Load += async (_, _) => { await CargarClientesAsync(); await CargarLotesAsync(); };
        }

        private async Task CargarClientesAsync()
        {
            try
            {
                var clientes = await _api.GetAsync<List<ClienteResponse>>("api/clientes");
                _cmbCliente.DataSource = clientes;
                _cmbCliente.DisplayMember = "RazonSocial";
                _cmbCliente.ValueMember = "Id";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarLotesAsync()
        {
            try
            {
                var lotes = await _api.GetAsync<List<LoteResponse>>("api/lotes");
                _grid.DataSource = lotes;
                if (_grid.Columns.Count > 0)
                {
                    _grid.Columns["Id"].Visible = false;
                    _grid.Columns["ClienteId"].Visible = false;
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCodigo.Text) || _cmbCliente.SelectedValue is null)
            {
                MessageBox.Show("Código y cliente son obligatorios.", "Datos incompletos");
                return;
            }

            try
            {
                await _api.PostAsync<LoteResponse>("api/lotes", new LoteCreateDto
                {
                    Codigo = _txtCodigo.Text.Trim(),
                    ClienteId = (int)_cmbCliente.SelectedValue,
                    Descripcion = _txtDescripcion.Text.Trim()
                });

                _txtCodigo.Clear();
                _txtDescripcion.Clear();
                await CargarLotesAsync();
                MessageBox.Show("Lote registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
