using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QualityLab.API.Data;
using QualityLab.API.Middleware;
using QualityLab.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1) Configuración fuertemente tipada de JWT
// ---------------------------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección JwtSettings en appsettings.json");

// ---------------------------------------------------------------------
// 2) Base de datos (SQL Server + EF Core)
// ---------------------------------------------------------------------
builder.Services.AddDbContext<QualityLabDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------
// 3) Servicios propios
// ---------------------------------------------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICertificadoService, CertificadoService>();

// ---------------------------------------------------------------------
// 4) Autenticación JWT ("¿Cómo se autenticó?")
// ---------------------------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // en producción: true
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1),

            // Permite que [Authorize(Roles = "ADMIN")] mapee correctamente los roles del token
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", p => p.RequireRole("ADMIN"));
    options.AddPolicy("StaffLaboratorio", p => p.RequireRole("ADMIN", "SUPERVISOR", "TECNICO"));
});

// ---------------------------------------------------------------------
// 5) CORS: permite comunicación de la Web (MVC), WinForms y Mobile.
// ---------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirClientes", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true);
    });
});

// ---------------------------------------------------------------------
// 6) Controladores + JsonStringEnumConverter (Acepta "ADMIN", "TECNICO", etc.)
// ---------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Permite recibir y responder enums como texto ("ADMIN") o como número (0)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QualityLab API",
        Version = "v1",
        Description = "API REST para el laboratorio de control de calidad industrial QualityLab."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresar únicamente el token JWT (sin el prefijo 'Bearer ')."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------------
// 7) Crear/migrar y sembrar la base de datos al iniciar
// ---------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<QualityLabDbContext>();
    DbInitializer.Seed(context);
}

// ---------------------------------------------------------------------
// 8) Pipeline HTTP
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QualityLab API v1"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTrackingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("PermitirClientes");

app.UseAuthentication(); // ¿Quién eres?
app.UseAuthorization();  // ¿Qué puedes hacer?

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "OK", servidor = "QualityLab.API", hora = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();