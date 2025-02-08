using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Controllers
{
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
