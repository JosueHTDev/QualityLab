using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QualityLab.Web.Models;
using QualityLab.Web.Services;

namespace QualityLab.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Muestras");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
        {
            try
            {
                var respuesta = await _api.PostAsync<LoginResponse>("api/auth/login", new LoginRequest
                {
                    NombreUsuario = modelo.NombreUsuario,
                    Password = modelo.Password
                });

                if (respuesta is null)
                {
                    modelo.Error = "No se pudo iniciar sesión.";
                    return View(modelo);
                }

                // Solo el rol CLIENTE usa este portal web.
                if (!string.Equals(respuesta.Rol, "CLIENTE", StringComparison.OrdinalIgnoreCase))
                {
                    modelo.Error = "Este portal es exclusivo para clientes. El personal del laboratorio debe usar la aplicación de escritorio.";
                    return View(modelo);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, respuesta.NombreUsuario),
                    new(ClaimTypes.Role, respuesta.Rol),
                    new("ApiToken", respuesta.Token) // JWT reenviado a la API en cada request (ver AuthHeaderHandler)
                };

                if (respuesta.ClienteId.HasValue)
                    claims.Add(new Claim("clienteId", respuesta.ClienteId.Value.ToString()));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = respuesta.ExpiraEn
                    });

                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Muestras");
            }
            catch (ApiException ex)
            {
                modelo.Error = ex.Message;
                return View(modelo);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
