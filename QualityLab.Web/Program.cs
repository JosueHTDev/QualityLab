using Microsoft.AspNetCore.Authentication.Cookies;
using QualityLab.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1) Configuración de la URL del API
// ---------------------------------------------------------------------
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
    ?? throw new InvalidOperationException("Falta la sección ApiSettings en appsettings.json");

// ---------------------------------------------------------------------
// 2) HttpClient tipado hacia QualityLab.API, con el handler que agrega
//    el JWT del usuario logueado (ver AuthHeaderHandler).
// ---------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient<ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiSettings.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Solo para desarrollo local con certificado autofirmado (https://localhost).
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

// ---------------------------------------------------------------------
// 3) Autenticación por cookie. El JWT de la API viaja dentro de la cookie
//    como claim "ApiToken" (ver AccountController.Login).
// ---------------------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = false; // la cookie expira junto con el JWT de la API
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
