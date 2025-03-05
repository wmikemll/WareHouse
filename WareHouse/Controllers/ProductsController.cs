using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WareHouse.Controllers
{
    [Authorize(Policy = "AdminPolicy, ProcurementManagerPolicy, SalesManagerPolicy, WarehouseWorkerPolicy, AccountantPolicy")] // Доступ для всех, кто может просматривать товары
    public class ProductsController : Controller
    {
        private readonly NeondbContext _dbContext; // Замените NeondbContext на имя вашего DbContext
        private readonly Random _random;

        public ProductsController(NeondbContext dbContext)
        {
            _random = new Random();
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = _dbContext.Categories.ToList();
            ViewBag.Products = _dbContext.Products.Include(p => p.Category).Where(p => p.isHidden == false).ToList();
            return View();
        }

        [Authorize(Policy = "AdminPolicy, ProcurementManagerPolicy")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult AddProduct(string Name, decimal Price, int Count, int Category)
        {
            _dbContext.Products.Add(new Models.Product()
            {
                Id = _random.Next(100000, 999999).ToString(),
                Name = Name,
                Price = Price,
                Count = Count,
                Categoryid = Category

            });
            _dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminPolicy, ProcurementManagerPolicy")] // Доступ только для Admin и ProcurementManager
        public IActionResult DeleteProduct(string id)
        {
            var product = _dbContext.Products.FirstOrDefault(x => x.Id.Trim() == id.Trim());
            if (product != null)
            {
                product.isHidden = true;
                _dbContext.SaveChanges();
            }
            else
                throw new Exception("Товар не найден для удаления, возможно проблемы с id");
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminPolicy, ProcurementManagerPolicy")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult EditProduct(string id, string name, decimal price, int count, int categoryId)
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.Id.Trim() == id.Trim());
            if (product == null)
            {
                return NotFound();
            }

            product.Name = name;
            product.Price = price;
            product.Count = count;
            product.Categoryid = categoryId;

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }


}
