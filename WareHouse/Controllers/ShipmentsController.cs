using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly NeondbContext _dbContext;

        public ShipmentsController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
