using QualityLab.WinForms.Forms;

namespace QualityLab.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // La aplicación siempre arranca pidiendo login.
            // Solo si el login es exitoso se abre el formulario principal.
            using var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainForm());
            }
        }
    }
}
