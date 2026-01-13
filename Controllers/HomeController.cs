using Microsoft.AspNetCore.Mvc;

namespace TourismManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
