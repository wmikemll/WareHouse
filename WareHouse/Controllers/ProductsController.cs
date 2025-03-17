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
            return View();
        }

		public async Task<IActionResult> DynamicSearch(string searchText, int? categoryId, decimal? minPrice, decimal? maxPrice, bool showLowStock)
		{
			IQueryable<Product> products = _dbContext.Products
				.Include(p => p.Category)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchText))
			{
				products = products.Where(p => p.Name.Contains(searchText));
			}

			if (categoryId.HasValue)
			{
				products = products.Where(p => p.Categoryid == categoryId.Value);
			}

			if (minPrice.HasValue)
			{
				products = products.Where(p => p.Price >= minPrice.Value);
			}

			if (maxPrice.HasValue)
			{
				products = products.Where(p => p.Price <= maxPrice.Value);
			}

			if (showLowStock)
			{
				products = products.Where(p => p.Count <= 10); // Порог низкого остатка
			}


			var filteredProducts = await products.ToListAsync();

			return PartialView("_ProductTablePartial", filteredProducts); // Return the Partial View
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

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
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

        [Authorize(Policy = "AdminOrProcurement")] // Доступ только для Admin и ProcurementManager
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
