using Microsoft.AspNetCore.Mvc;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    public class EmployeesController : Controller
    {
        public IActionResult Index()
        {
            List<User> users = [new User() { Id = "1234", Name = "Эмиль" }];
            return View(users);
        }
    }
}
