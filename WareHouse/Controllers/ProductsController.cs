using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly NeondbContext _DbContext; 
        private readonly Random _random;

        public ProductsController(NeondbContext dbContext)
        {
            _random = new Random();
            _DbContext = dbContext;
        }

        public IActionResult Index(bool isHidden = false)
        {
            ViewBag.ShowHidden = isHidden;
            ViewBag.Categories = _DbContext.MaterialTypes.Where(c => c.IsDeleted == false).ToList();
            return View();
        }

        public IActionResult DynamicSearch(string searchText, int? categoryId, bool isHidden) //поиск товаров через строку
        {
            var products = _DbContext.Products
                .Include(p => p.Materialtype).Where(p => p.isHidden == isHidden)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchText.ToLower()));
            }

            if (categoryId.HasValue)
            {

                products = products.Where(p => p.MaterialTypeId == categoryId);
            }
 
            var productList = products.ToList();
            return PartialView("_ProductTablePartial", productList);
        }

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult AddProduct(string Name, decimal Price, string SpecificGravity, string Weight, int MaterialType, string MaterialBrand)
        {
            _DbContext.Products.Add(new Models.Product()
            {
                Id = _random.Next(100000, 999999).ToString(),
                Name = Name,
                Price = Price,
                SpecificGravity = double.Parse(SpecificGravity, CultureInfo.InvariantCulture),
                Weight = double.Parse(Weight, CultureInfo.InvariantCulture),
                MaterialTypeId = MaterialType,
                MaterialBrand = MaterialBrand
            });
            _DbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
        public IActionResult DeleteProduct(string id)
        {
            var product = _DbContext.Products.FirstOrDefault(x => x.Id.Trim() == id.Trim());
            if (product != null)
            {
                product.isHidden = true;
                _DbContext.SaveChanges();
            }
            else
                throw new Exception("Товар не найден для удаления, возможно проблемы с id");
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult EditProduct(string id, string name, decimal price, string SpecificGravity, string Weight, int MaterialType, string MaterialBrand)
        {
            var product = _DbContext.Products.FirstOrDefault(p => p.Id.Trim() == id.Trim());
            if (product == null)
            {
                return NotFound();
            }

            product.Name = name;
            product.Price = price;
            product.MaterialTypeId = MaterialType;
            product.MaterialBrand = MaterialBrand;
            product.SpecificGravity = double.Parse(SpecificGravity, CultureInfo.InvariantCulture);
            product.Weight = double.Parse(Weight, CultureInfo.InvariantCulture);
            _DbContext.Update(product);
            _DbContext.SaveChanges();

            return RedirectToAction("Index");
        }
        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult SaveCategory(Materialtype category)
        {
            if (category.Id == 0)
            {
                // Добавление новой категории
                category.Id = _DbContext.MaterialTypes.ToList().Count+1;
                _DbContext.MaterialTypes.Add(category);
            }
            else
            {
                // Редактирование существующей категории
                var existingCategory = _DbContext.MaterialTypes.Find(category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                }
            }
            _DbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _DbContext.MaterialTypes.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound("Категория не найдена");
            }

            bool isCategoryInUse = _DbContext.Products.Any(p => p.MaterialTypeId == id && p.Weight > 0 && !p.isHidden);

            if (isCategoryInUse)
            {
                return BadRequest("Категория используется в товарах и не может быть удалена");
            }

            try
            {
                category.IsDeleted = true;
                _DbContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла ошибка при удалении");
            }
        }
    }


}
