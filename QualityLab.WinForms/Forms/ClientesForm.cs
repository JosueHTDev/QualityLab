using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class ClientesForm : Form
    {
        private readonly ApiClient _api = new();
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        private readonly TextBox _txtRazonSocial = new() { Left = 130, Top = 20, Width = 250 };
        private readonly TextBox _txtRuc = new() { Left = 130, Top = 50, Width = 150 };
        private readonly TextBox _txtEmail = new() { Left = 130, Top = 80, Width = 250 };
        private readonly TextBox _txtTelefono = new() { Left = 130, Top = 110, Width = 150 };
        private readonly TextBox _txtDireccion = new() { Left = 130, Top = 140, Width = 350 };
        private readonly Button _btnGuardar = new() { Text = "Guardar", Left = 130, Top = 175, Width = 100 };
        private readonly Button _btnRefrescar = new() { Text = "Refrescar lista", Left = 240, Top = 175, Width = 120 };

        public ClientesForm()
        {
            Text = "Gestión de Clientes";
            Width = 750;
            Height = 500;

            var panelForm = new Panel { Dock = DockStyle.Top, Height = 220 };
            panelForm.Controls.Add(new Label { Text = "Razón social:", Left = 20, Top = 23, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "RUC:", Left = 20, Top = 53, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Email:", Left = 20, Top = 83, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Teléfono:", Left = 20, Top = 113, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Dirección:", Left = 20, Top = 143, Width = 100 });
            panelForm.Controls.Add(_txtRazonSocial);
            panelForm.Controls.Add(_txtRuc);
            panelForm.Controls.Add(_txtEmail);
            panelForm.Controls.Add(_txtTelefono);
            panelForm.Controls.Add(_txtDireccion);
            panelForm.Controls.Add(_btnGuardar);
            panelForm.Controls.Add(_btnRefrescar);

            Controls.Add(_grid);
            Controls.Add(panelForm);

            _btnGuardar.Click += BtnGuardar_Click;
            _btnRefrescar.Click += async (_, _) => await CargarClientesAsync();
            Load += async (_, _) => await CargarClientesAsync();
        }

        private async Task CargarClientesAsync()
        {
            try
            {
                var clientes = await _api.GetAsync<List<ClienteResponse>>("api/clientes");
                _grid.DataSource = clientes;
                if (_grid.Columns.Count > 0)
                    _grid.Columns["Id"].Visible = false;
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtRazonSocial.Text) || string.IsNullOrWhiteSpace(_txtRuc.Text))
            {
                MessageBox.Show("Razón social y RUC son obligatorios.", "Datos incompletos");
                return;
            }

            try
            {
                await _api.PostAsync<ClienteResponse>("api/clientes", new ClienteCreateDto
                {
                    RazonSocial = _txtRazonSocial.Text.Trim(),
                    RUC = _txtRuc.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    Telefono = _txtTelefono.Text.Trim(),
                    Direccion = _txtDireccion.Text.Trim()
                });

                LimpiarFormulario();
                await CargarClientesAsync();
                MessageBox.Show("Cliente registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            _txtRazonSocial.Clear();
            _txtRuc.Clear();
            _txtEmail.Clear();
            _txtTelefono.Clear();
            _txtDireccion.Clear();
        }
    }
}
