using Microsoft.AspNetCore.Mvc;
using WebApplication1.Helpers;

namespace WebApplication1.Areas.PrivateSite.Controllers
{
    [Area("PrivateSite")]
    [CustomAuthorize(Roles = "Admin")]

    public class Dashboard : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
