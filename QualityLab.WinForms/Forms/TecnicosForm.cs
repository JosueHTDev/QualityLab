using QualityLab.WinForms.Models;
using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class TecnicosForm : Form
    {
        private readonly ApiClient _api = new();
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };

        private readonly TextBox _txtNombres = new() { Left = 130, Top = 20, Width = 200 };
        private readonly TextBox _txtApellidos = new() { Left = 130, Top = 50, Width = 200 };
        private readonly TextBox _txtEspecialidad = new() { Left = 130, Top = 80, Width = 250 };
        private readonly TextBox _txtEmail = new() { Left = 130, Top = 110, Width = 250 };
        private readonly TextBox _txtTelefono = new() { Left = 130, Top = 140, Width = 150 };
        private readonly Button _btnGuardar = new() { Text = "Guardar", Left = 130, Top = 175, Width = 100 };
        private readonly Button _btnRefrescar = new() { Text = "Refrescar lista", Left = 240, Top = 175, Width = 120 };

        public TecnicosForm()
        {
            Text = "Gestión de Técnicos";
            Width = 750;
            Height = 500;

            var panelForm = new Panel { Dock = DockStyle.Top, Height = 220 };
            panelForm.Controls.Add(new Label { Text = "Nombres:", Left = 20, Top = 23, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Apellidos:", Left = 20, Top = 53, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Especialidad:", Left = 20, Top = 83, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Email:", Left = 20, Top = 113, Width = 100 });
            panelForm.Controls.Add(new Label { Text = "Teléfono:", Left = 20, Top = 143, Width = 100 });
            panelForm.Controls.Add(_txtNombres);
            panelForm.Controls.Add(_txtApellidos);
            panelForm.Controls.Add(_txtEspecialidad);
            panelForm.Controls.Add(_txtEmail);
            panelForm.Controls.Add(_txtTelefono);
            panelForm.Controls.Add(_btnGuardar);
            panelForm.Controls.Add(_btnRefrescar);

            Controls.Add(_grid);
            Controls.Add(panelForm);

            _btnGuardar.Click += BtnGuardar_Click;
            _btnRefrescar.Click += async (_, _) => await CargarTecnicosAsync();
            Load += async (_, _) => await CargarTecnicosAsync();
        }

        private async Task CargarTecnicosAsync()
        {
            try
            {
                var tecnicos = await _api.GetAsync<List<TecnicoResponse>>("api/tecnicos");
                _grid.DataSource = tecnicos;
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
            if (string.IsNullOrWhiteSpace(_txtNombres.Text) || string.IsNullOrWhiteSpace(_txtApellidos.Text))
            {
                MessageBox.Show("Nombres y apellidos son obligatorios.", "Datos incompletos");
                return;
            }

            try
            {
                await _api.PostAsync<TecnicoResponse>("api/tecnicos", new TecnicoCreateDto
                {
                    Nombres = _txtNombres.Text.Trim(),
                    Apellidos = _txtApellidos.Text.Trim(),
                    Especialidad = _txtEspecialidad.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    Telefono = _txtTelefono.Text.Trim()
                });

                LimpiarFormulario();
                await CargarTecnicosAsync();
                MessageBox.Show("Técnico registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            _txtNombres.Clear();
            _txtApellidos.Clear();
            _txtEspecialidad.Clear();
            _txtEmail.Clear();
            _txtTelefono.Clear();
        }
    }
}
