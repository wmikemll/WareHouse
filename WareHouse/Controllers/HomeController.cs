using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;
using System.Text;

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
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (User.Identity.IsAuthenticated)
            {
                return View(); 
            }

            ViewBag.ShowModal = true; 
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string mail, string password)
        {
            var account = _dbContext.Accounts
                .Include(a => a.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefault(u => u.Mail.Trim() == mail.Trim());

            if (account == null || account.User == null)
            {
                ViewBag.ErrorMessage = "Пользователь не найден";
                ViewBag.ShowModal = true;
                return View();
            }

            bool isPasswordValid = VerifyPassword(password, account.Password);

            if (!isPasswordValid)
            {
                ViewBag.ErrorMessage = "Пароль неверный";
                ViewBag.ShowModal = true;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{account.User.Surname} {account.User.Name}"),
                new Claim(ClaimTypes.Email, mail),
                new Claim(ClaimTypes.Role, account.User.Role.Name),
                new Claim(ClaimTypes.NameIdentifier, account.Userid.ToString()),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true 
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        private bool VerifyPassword(string password, string passwordHash) //проверка пароля
        {
            return password == Decrypt(passwordHash);
        }

        public IActionResult Logout() //выход из аккаунта
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public static string Decrypt(string cipherText) //Расшифрование пароля
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            return Encoding.UTF8.GetString(cipherBytes);
        }



    }
}