# QualityLab.Web

Portal web (ASP.NET Core MVC) para que los **clientes** del laboratorio:
- Consulten sus muestras.
- Consulten el estado de una muestra.
- Consulten el resultado (cuando ya está disponible).
- Descarguen el certificado emitido.

## Cómo funciona la autenticación

1. El usuario hace login en `/Account/Login`. El formulario envía las
   credenciales a `QualityLab.API` (`POST /api/auth/login`).
2. Si el login es válido **y el rol es CLIENTE**, se crea una cookie de
   autenticación (`CookieAuthenticationDefaults`) cuyo claim `ApiToken`
   guarda el JWT que entregó la API.
3. En cada request posterior, `AuthHeaderHandler` toma ese JWT de la cookie
   y lo reenvía como `Authorization: Bearer {token}` hacia la API, además
   de identificar la app con el header `X-Client-App: QualityLab.Web`.
4. Como la API ya filtra los datos según el `clienteId` embebido en el
   token, este proyecto **no duplica ninguna regla de seguridad**: solo
   muestra lo que la API decide devolver.

Este portal es exclusivo para el rol **CLIENTE**. El personal del
laboratorio (ADMIN/SUPERVISOR/TECNICO) usa la aplicación WinForms.

## Ejecutar

1. Asegúrate de que `QualityLab.API` esté corriendo (por defecto en
   `https://localhost:5081`). Si usas otro puerto, ajústalo en
   `appsettings.json` → `ApiSettings:BaseUrl`.
2. Desde esta carpeta:

   ```bash
   dotnet restore
   dotnet run
   ```

3. Abre la URL que indique la consola e inicia sesión con el usuario de
   prueba `cliente1` / `Cliente123!` (sembrado por `DbInitializer` en el API).

## Estructura

```
QualityLab.Web/
├── Controllers/
│   ├── AccountController.cs   Login/Logout contra la API
│   ├── MuestrasController.cs  Consultar muestra/estado/resultado/certificado
│   └── HomeController.cs      Redirección inicial y página de error
├── Services/
│   ├── ApiClient.cs           Wrapper tipado de HttpClient hacia la API
│   ├── AuthHeaderHandler.cs   Agrega el JWT y el header X-Client-App
│   └── ApiSettings.cs / ApiException.cs
├── Models/                    DTOs espejo del API + ViewModels
└── Views/                     Razor views (Bootstrap vía CDN)
```
