using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    public class HomeController : Controller
    {
        private readonly NeondbContext _dbContext;

        public HomeController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous] // Allow anonymous access to the Login POST action
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string mail, string password)
        {
            // 1. Authentication Logic
            var account = _dbContext.Accounts.FirstOrDefault(u => u.Mail == mail && u.Password == password);

            if (account == null)
            {
                ViewBag.ErrorMessage = "Неверный email или пароль.";  // Set an error message
                return View();
            }

            // 2. Authentication (Create Claims & SignIn)

            // 3. Success - Redirect
            return RedirectToAction("Index", "Home");
        }

    }
}
