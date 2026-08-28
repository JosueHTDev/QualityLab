using QualityLab.Mobil.Pages;

namespace QualityLab.Mobil
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MuestrasAsignadasPage), typeof(MuestrasAsignadasPage));
            Routing.RegisterRoute(nameof(DetalleMuestraPage), typeof(DetalleMuestraPage));
            Routing.RegisterRoute(nameof(RegistrarAvancePage), typeof(RegistrarAvancePage));
            Routing.RegisterRoute(nameof(RegistrarIncidenciaPage), typeof(RegistrarIncidenciaPage));
            Routing.RegisterRoute(nameof(PendientesPage), typeof(PendientesPage));
        }
    }
}