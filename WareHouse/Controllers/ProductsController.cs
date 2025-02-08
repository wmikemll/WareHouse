using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Controllers
{
    public class ProductsController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }


}
