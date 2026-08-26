# QualityLab.API

API REST principal del sistema **QualityLab** (laboratorio de control de calidad
industrial), construida en **.NET 8 / ASP.NET Core Web API**, con **EF Core +
SQL Server**, autenticación **JWT**, contraseñas con **BCrypt** y documentación
con **Swagger**. Esta API es el backend único que consumirán las tres
aplicaciones cliente del caso (WinForms, Web MVC y MAUI) usando `HttpClient`.

## 1. Requisitos previos

- .NET 8 SDK
- SQL Server (local, Docker o Azure SQL). También funciona con
  `(localdb)\MSSQLLocalDB` en Windows.

## 2. Configuración

1. Abrir `appsettings.json` y ajustar `ConnectionStrings:DefaultConnection`
   con los datos de tu SQL Server. Ejemplo con LocalDB:

   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QualityLabDB;Trusted_Connection=True;TrustServerCertificate=True"
   ```

2. (Opcional pero recomendado) mover `JwtSettings:SecretKey` a *user-secrets*
   o variables de entorno antes de subir el proyecto a un repositorio:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:SecretKey" "otra-clave-larga-y-secreta"
   ```

## 3. Ejecutar el proyecto

```bash
cd QualityLab.API
dotnet restore
dotnet run
```

Al iniciar, el `Program.cs` llama a `DbInitializer.Seed(context)`, que:
- Crea la base de datos si no existe (`EnsureCreated`).
- Siembra un cliente, un técnico y **cuatro usuarios de prueba**, uno por rol.

Swagger queda disponible en `https://localhost:5081/swagger` (o el puerto que
asigne tu máquina — revisa la consola).

### Usuarios sembrados (para probar cada rol)

| Usuario       | Password        | Rol        |
|---------------|-----------------|------------|
| `admin`       | `Admin123!`     | ADMIN      |
| `supervisor1` | `Supervisor123!`| SUPERVISOR |
| `tecnico1`    | `Tecnico123!`   | TECNICO    |
| `cliente1`    | `Cliente123!`   | CLIENTE    |

Ya existe además un lote (`LOTE-2025-001`), una muestra (`MUE-2025-0001`)
asignada a `tecnico1`, y un análisis en proceso, para poder probar el flujo
completo sin tener que crear todo desde cero.

## 4. Flujo típico de prueba

1. `POST /api/auth/login` con `admin` → obtienes el token.
2. En Swagger, botón **Authorize**, pegar el token (sin la palabra `Bearer`).
3. Crear/consultar clientes, lotes, muestras, asignar técnico.
4. Loguearte como `tecnico1`, completar el análisis
   (`PUT /api/analisis/{id}/completar`) y registrar un resultado
   (`POST /api/resultados`).
5. Como `admin` o `supervisor1`, emitir el certificado
   (`POST /api/certificados/muestra/{muestraId}/emitir`).
6. Loguearte como `cliente1` y consultar estado / descargar el certificado.

## 5. Respuestas a las preguntas de arquitectura del caso

**¿Qué aplicación hizo la petición?**
Cada cliente (WinForms, Web MVC, MAUI) envía el header `X-Client-App` (por
ejemplo `QualityLab.WinForms`, `QualityLab.Web`, `QualityLab.Mobile`). El
`RequestTrackingMiddleware` lo lee, lo registra en el log y lo expone en
`HttpContext.Items["ClientApp"]`.

**¿Qué API recibió la petición?**
Siempre esta única API (`QualityLab.API`), que centraliza toda la lógica de
negocio y el acceso a datos. El nombre de la API y la ruta invocada quedan en
el log de `RequestTrackingMiddleware`, y el header de respuesta
`X-Api-Name` lo confirma al cliente.

**¿Qué middleware intervino?**
El pipeline definido en `Program.cs`, en este orden:
`ExceptionHandlingMiddleware` → `RequestTrackingMiddleware` →
`UseHttpsRedirection` → `UseCors` → `UseAuthentication` → `UseAuthorization`
→ Controladores. Cada petición queda con un `X-Correlation-Id` que aparece
tanto en la respuesta como en los logs de consola, permitiendo rastrear por
qué middleware pasó.

**¿Cómo se autenticó?**
Con JWT (JSON Web Token). El usuario envía credenciales a
`POST /api/auth/login`; el servidor valida el hash BCrypt de la contraseña y,
si es correcto, firma un token con `HMACSHA256` que incluye claims de
identidad, rol (`ADMIN`/`SUPERVISOR`/`TECNICO`/`CLIENTE`) y, según el caso,
`clienteId` o `tecnicoId`. Ese token se envía luego en el header
`Authorization: Bearer {token}` en cada petición protegida.

**¿Dónde se almacenó?**
Todo en **SQL Server**, vía EF Core (`QualityLabDbContext`). Los certificados
se guardan como `varbinary(max)` en la tabla `Certificados` (no en el disco
del servidor), para que cualquier instancia de la API pueda servirlos.

**¿Cómo se sincronizó?**
Los clientes WinForms y Web trabajan siempre en línea contra la API. El
cliente móvil (MAUI) puede operar offline: `RegistrarAvanceDto` y
`RegistrarIncidenciaDto` aceptan un `IdLocalOrigen` (GUID generado en el
dispositivo). Cuando vuelve la conexión, la app reenvía esos registros; el
servidor detecta el `IdLocalOrigen` ya existente (índice único filtrado en
`Incidencias`/`Avances`) y responde sin duplicar el dato — sincronización
idempotente.

**¿Qué ocurre si se pierde Internet?**
La API expone `GET /api/health` (sin autenticación) para que los clientes
verifiquen conectividad. Si no hay respuesta, WinForms/MAUI deben guardar la
operación localmente (SQLite local, cola en memoria, etc.) y reintentar el
envío cuando `/api/health` vuelva a responder. La API en sí no necesita saber
que hubo una caída: solo recibe, al reconectar, las operaciones pendientes
con su `IdLocalOrigen`.

## 6. Guía rápida de las 10 pruebas

| # | Prueba | Cómo probarla |
|---|--------|----------------|
| 1 | Login correcto | `POST /api/auth/login` con `admin` / `Admin123!` → `200 OK` + token |
| 2 | Login incorrecto | Mismo endpoint con password errada → `401 Unauthorized` |
| 3 | Consulta sin token | `GET /api/clientes` sin header `Authorization` → `401 Unauthorized` |
| 4 | Token válido | Repetir la petición anterior con `Authorization: Bearer {token}` → `200 OK` |
| 5 | Rol incorrecto | Loguearte como `cliente1` y llamar `GET /api/clientes` (solo ADMIN/SUPERVISOR) → `403 Forbidden` |
| 6 | Comunicación entre aplicaciones | Enviar header `X-Client-App: QualityLab.Web` (o WinForms/Mobile) y revisar el log de consola + header `X-Correlation-Id` en la respuesta |
| 7 | Pérdida de conexión | Apagar la API y hacer `GET /api/health` desde el cliente → debe fallar; el cliente guarda la operación localmente |
| 8 | Sincronización posterior | Levantar la API y reenviar `POST /api/tecnico/avances` con el mismo `IdLocalOrigen` dos veces → la segunda vez responde "ya sincronizado", sin duplicar |
| 9 | Middleware funcionando | Revisar la consola: cada petición imprime `[correlationId] Peticion recibida...` y `...finalizada` con status y duración |
| 10 | Persistencia de información | Reiniciar la API (sin borrar la BD) y confirmar con `GET /api/muestras` que los datos siguen ahí (persistidos en SQL Server, no en memoria) |

## 7. Estructura del proyecto

```
QualityLab.API/
├── Controllers/        Endpoints REST (uno por entidad + Auth + operaciones técnico)
├── Data/                DbContext y seeder inicial
├── Middleware/          Trazabilidad y manejo global de excepciones
├── Models/
│   ├── Entities/        Entidades EF Core
│   ├── Enums/           Roles y estados del flujo
│   └── DTOs/            Contratos de entrada/salida de la API
├── Services/            Generación de JWT y de certificados
├── Program.cs           Configuración y pipeline
└── appsettings.json     Connection string y configuración JWT
```

## 8. Próximos pasos (no incluidos en esta entrega)

- Proyecto WinForms (flujo de laboratorio interno).
- Proyecto ASP.NET Core MVC (portal del cliente).
- Proyecto .NET MAUI (app del técnico, con almacenamiento local SQLite para
  modo offline real).
- Generación de certificados en PDF real (por ejemplo con QuestPDF) en lugar
  del texto plano actual.
- Migraciones EF Core explícitas (`dotnet ef migrations add InitialCreate`)
  en vez de `EnsureCreated`, si el proyecto va a evolucionar en equipo.
