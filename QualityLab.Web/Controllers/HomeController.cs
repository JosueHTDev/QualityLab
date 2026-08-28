using Microsoft.AspNetCore.Mvc;

namespace QualityLab.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return User.Identity?.IsAuthenticated == true
                ? RedirectToAction("Index", "Muestras")
                : RedirectToAction("Login", "Account");
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
