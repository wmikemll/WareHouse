using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    public class HomeController : Controller
    {
        private readonly NeondbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Check if the user is already authenticated
            if (User.Identity.IsAuthenticated)
            {
                return View(); // Return the normal view if authenticated
            }

            ViewBag.ShowModal = true; // Set a flag to show the modal
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string mail, string password)
        {
            var account = _dbContext.Accounts.Include(a => a.User).FirstOrDefault(u => u.Mail.Trim() == mail && u.Password.Trim() == password);

            if (account != null && account.User != null)
            {
                // 2. Authentication (Create Claims & SignIn)
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{account.User.Surname} {account.User.Name}"),
                new Claim(ClaimTypes.Email, mail),
                new Claim(ClaimTypes.NameIdentifier, account.Userid)
            };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // "Remember Me" functionality
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // 3. Success - Redirect
                return RedirectToAction("Index", "Home"); // Redirect to the same page (or another secure page)
            }
            else
            {
                //Authentication failed
                ViewBag.ErrorMessage = "Неверная почта или пароль";
                ViewBag.ShowModal = true;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }



    }
}