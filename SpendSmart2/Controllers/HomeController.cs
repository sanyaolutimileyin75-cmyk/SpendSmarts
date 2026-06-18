using Microsoft.AspNetCore.Mvc;

namespace SpendSmart2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
