using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    [Authorize] // Требуется аутентификация для доступа к контроллеру
    public class SalesController : Controller
    {
        private readonly NeondbContext _dbContext; // Замените NeondbContext на имя вашего DbContext
        private readonly Random random;

        public SalesController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
            random = new Random();
        }

        [Authorize(Policy = "FullAccess")]
        public async Task<IActionResult> Index()
        {
            var sales = await _dbContext.Sales
                .Include(s => s.Status)
                .Include(s => s.User)
                .Include(s => s.Saleitems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.IsHidden == false)
                .ToListAsync();

            Dictionary<string, string> saleItemsJson = new Dictionary<string, string>();

            foreach (var sale in sales)
            {
                string json = JsonConvert.SerializeObject(sale.Saleitems, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                saleItemsJson[sale.Id] = json;
            }


            ViewBag.Sales = sales;
            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
            ViewBag.Users = await _dbContext.Users.ToListAsync();
            ViewBag.Products = await _dbContext.Products.ToListAsync();
            ViewBag.SaleItemsJson = saleItemsJson;

            return View();
        }

        [Authorize(Policy = "AdminPolicy, SalesManagerPolicy")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
            ViewBag.Users = await _dbContext.Users.ToListAsync();
            ViewBag.Products = await _dbContext.Products.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy, SalesManagerPolicy")]
        public async Task<IActionResult> Create(string UserId, string[] ProductIds, Dictionary<string, int> Quantities)
        {
            if (ModelState.IsValid)
            {
                var sale = new Sale
                {
                    Id = random.Next(100000, 999999).ToString(),
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    StatusId = 5,
                    Userid = UserId
                };

                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && Quantities[productId] > 0)
                    {
                        int quantity = Quantities[productId];

                        var stockItem = await _dbContext.Products.FirstOrDefaultAsync(s => s.Id == productId);
                        if (stockItem == null || stockItem.Count < quantity)
                        {
                            ModelState.AddModelError("", $"Недостаточно товара '{_dbContext.Products.Find(productId).Name}' на складе.");
                            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
                            ViewBag.Users = await _dbContext.Users.ToListAsync();
                            ViewBag.Products = await _dbContext.Products.ToListAsync();
                            return View();
                        }

                        stockItem.Count -= quantity;
                        _dbContext.Update(stockItem);

                        sale.Saleitems.Add(new Saleitem
                        {
                            Id = random.Next(100000, 999999).ToString(),
                            Productid = productId,
                            Saleid = sale.Id,
                            Count = quantity
                        });
                    }
                }

                _dbContext.Sales.Add(sale);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
            ViewBag.Users = await _dbContext.Users.ToListAsync();
            ViewBag.Products = await _dbContext.Products.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy, SalesManagerPolicy")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sale = await _dbContext.Sales
                .Include(s => s.Saleitems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            int canceledStatusId = 3;

            foreach (var saleItem in sale.Saleitems)
            {
                var stockItem = await _dbContext.Products.FirstOrDefaultAsync(s => s.Id == saleItem.Productid);
                if (stockItem != null)
                {
                    stockItem.Count += saleItem.Count;
                    _dbContext.Update(stockItem);
                }
            }

            sale.StatusId = canceledStatusId;
            _dbContext.Update(sale);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при отмене продажи.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Hide(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sale = await _dbContext.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }
            sale.IsHidden = true;
            _dbContext.Update(sale);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при изменении статуса продажи.");
            }

            return RedirectToAction(nameof(Index));
        }
    }

}

