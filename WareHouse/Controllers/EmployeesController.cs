using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    [Authorize(Policy = "AdminOnly")] // Доступ только для Admin
    public class EmployeesController : Controller
    {
        private readonly NeondbContext _dbContext; // Замените NeondbContext на имя вашего DbContext
        private readonly Random _random;

        public EmployeesController(NeondbContext dbContext)
        {
            _random = new Random();
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.Accounts = _dbContext.Accounts.Include(e => e.User).Where(a => a.Isactive == true).ToList();
            ViewBag.Roles = _dbContext.Roles.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddEmployee(string Name, string Surname, string Patronomic, string Email, string Password, string Phone, int RoleId)
        {
            var employee = new User()
            {
                Id = _random.Next(100000, 999999).ToString(),
                Name = Name,
                Surname = Surname,
                Patronomic = Patronomic,
                Roleid = RoleId
            };
            _dbContext.Users.Add(employee);
            _dbContext.SaveChanges();
            _dbContext.Accounts.Add(new Account()
            {
                Id = _random.Next(100000, 999999).ToString(),
                Mail = Email,
                Password = Password,
                CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                Isactive = true,
                Phone = Phone,
                Userid = employee.Id
            });
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteEmployee(string id)
        {
            var account = _dbContext.Accounts.FirstOrDefault(x => x.Userid == id);
            if (account != null)
            {
                account.Isactive = false;
                _dbContext.SaveChanges();
            }
            else
                throw new Exception("Сотрудник не найден для удаления, возможно проблемы с id");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditEmployee(string id, string Name, string Surname, string Patronomic, string Email, string Password, string Phone, int RoleId)
        {
            var account = _dbContext.Accounts.FirstOrDefault(x => x.Userid == id);
            var employee = _dbContext.Users.FirstOrDefault(x => x.Id.Trim() == account.Userid);
            if (account == null && employee == null)
            {
                return NotFound();
            }
            employee.Name = Name;
            employee.Surname = Surname;
            employee.Patronomic = Patronomic;
            employee.Roleid = RoleId;
            account.Phone = Phone;
            account.Mail = Email;
            account.Password = Password;
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
