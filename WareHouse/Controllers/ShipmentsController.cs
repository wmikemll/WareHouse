using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Controllers
{
    public class ShipmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
