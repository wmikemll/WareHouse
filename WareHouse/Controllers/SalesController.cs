using Microsoft.AspNetCore.Mvc;

namespace WareHouse.Controllers
{
    public class SalesController : Controller
    {
        private readonly NeondbContext _dbContext;

        public SalesController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
