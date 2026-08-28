using Microsoft.Extensions.Logging;
using QualityLab.Mobil;
using QualityLab.Mobil.Services;

namespace QualityLab.Mobil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<LocalQueueService>();
            builder.Services.AddSingleton<SyncService>();

            builder.Services.AddTransient<Pages.LoginPage>();
            builder.Services.AddTransient<Pages.MuestrasAsignadasPage>();
            builder.Services.AddTransient<Pages.DetalleMuestraPage>();
            builder.Services.AddTransient<Pages.RegistrarAvancePage>();
            builder.Services.AddTransient<Pages.RegistrarIncidenciaPage>();
            builder.Services.AddTransient<Pages.PendientesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}