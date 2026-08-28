using QualityLab.WinForms.Services;

namespace QualityLab.WinForms.Forms
{
    public class MainForm : Form
    {
        private readonly MenuStrip _menu = new();
        private readonly StatusStrip _statusStrip = new();
        private readonly ToolStripStatusLabel _lblSesion = new();

        public MainForm()
        {
            Text = "QualityLab - Laboratorio de Control de Calidad Industrial";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            ConstruirMenu();
            ConstruirBarraEstado();

            Controls.Add(_menu);
            Controls.Add(_statusStrip);
            MainMenuStrip = _menu;
        }

        private void ConstruirMenu()
        {
            // ---- Maestros (solo ADMIN) ----
            if (SessionManager.TieneRol("ADMIN"))
            {
                var menuMaestros = new ToolStripMenuItem("Maestros");
                menuMaestros.DropDownItems.Add("Clientes", null, (_, _) => AbrirFormulario(new ClientesForm()));
                menuMaestros.DropDownItems.Add("Técnicos", null, (_, _) => AbrirFormulario(new TecnicosForm()));
                _menu.Items.Add(menuMaestros);
            }

            // ---- Laboratorio (flujo: Muestra -> Asignación -> Análisis -> Resultado) ----
            var menuLaboratorio = new ToolStripMenuItem("Laboratorio");

            if (SessionManager.TieneRol("ADMIN", "SUPERVISOR"))
                menuLaboratorio.DropDownItems.Add("Lotes", null, (_, _) => AbrirFormulario(new LotesForm()));

            menuLaboratorio.DropDownItems.Add("Muestras", null, (_, _) => AbrirFormulario(new MuestrasForm()));
            menuLaboratorio.DropDownItems.Add("Análisis", null, (_, _) => AbrirFormulario(new AnalisisForm()));
            menuLaboratorio.DropDownItems.Add("Resultados", null, (_, _) => AbrirFormulario(new ResultadosForm()));

            if (SessionManager.TieneRol("ADMIN", "SUPERVISOR"))
                menuLaboratorio.DropDownItems.Add("Certificados", null, (_, _) => AbrirFormulario(new CertificadosForm()));

            _menu.Items.Add(menuLaboratorio);

            // ---- Sesión ----
            var menuSesion = new ToolStripMenuItem("Sesión");
            menuSesion.DropDownItems.Add("Cerrar sesión", null, (_, _) => CerrarSesion());
            _menu.Items.Add(menuSesion);
        }

        private void ConstruirBarraEstado()
        {
            _lblSesion.Text = $"Conectado como: {SessionManager.NombreUsuario}   |   Rol: {SessionManager.Rol}   |   API: {AppConfig.BaseUrl}";
            _statusStrip.Items.Add(_lblSesion);
        }

        private static void AbrirFormulario(Form form)
        {
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void CerrarSesion()
        {
            SessionManager.CerrarSesion();
            Hide();

            using var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // Reconstruir el menú porque el rol pudo cambiar
                _menu.Items.Clear();
                ConstruirMenu();
                _lblSesion.Text = $"Conectado como: {SessionManager.NombreUsuario}   |   Rol: {SessionManager.Rol}   |   API: {AppConfig.BaseUrl}";
                Show();
            }
            else
            {
                Close();
            }
        }
    }
}
