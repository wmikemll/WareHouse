using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WareHouse.Models;

namespace WareHouse.Controllers
{
    public class SalesController : Controller
    {
        private readonly NeondbContext _dbContext;
        private readonly Random random;

        public SalesController(NeondbContext dbContext)
        {
            _dbContext = dbContext;
            random = new Random();
        }
        public async Task<IActionResult> Index()
        {
            var sales = await _dbContext.Sales
                .Include(s => s.Status)
                .Include(s => s.User)
                .Include(s => s.Saleitems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.IsHidden == false)
                .ToListAsync();

            // Создаем словарь для хранения JSON-представлений SaleItems
            Dictionary<string, string> saleItemsJson = new Dictionary<string, string>();

            foreach (var sale in sales)
            {
                // Сериализуем Saleitems в JSON
                string json = JsonConvert.SerializeObject(sale.Saleitems, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                // Сохраняем JSON в словаре
                saleItemsJson[sale.Id] = json;
            }


            ViewBag.Sales = sales;
            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
            ViewBag.Users = await _dbContext.Users.ToListAsync();
            ViewBag.Products = await _dbContext.Products.ToListAsync();
            ViewBag.SaleItemsJson = saleItemsJson;

            return View();
        }

        // GET: Sales/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
            ViewBag.Users = await _dbContext.Users.ToListAsync();
            ViewBag.Products = await _dbContext.Products.ToListAsync();
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string UserId, string[] ProductIds, Dictionary<string, int> Quantities)
        {
            if (ModelState.IsValid)
            {
                // Создаем новую продажу
                var sale = new Sale
                {
                    Id = random.Next(100000, 999999).ToString(),
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    StatusId = 5,
                    Userid = UserId
                };

                // Добавляем товары в продажу
                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && Quantities[productId] > 0)
                    {
                        int quantity = Quantities[productId];

                        // Проверяем наличие товара на складе
                        var stockItem = await _dbContext.Products.FirstOrDefaultAsync(s => s.Id == productId);
                        if (stockItem == null || stockItem.Count < quantity)
                        {
                            ModelState.AddModelError("", $"Недостаточно товара '{_dbContext.Products.Find(productId).Name}' на складе.");
                            ViewBag.Statuses = await _dbContext.Statuses.ToListAsync();
                            ViewBag.Users = await _dbContext.Users.ToListAsync();
                            ViewBag.Products = await _dbContext.Products.ToListAsync();
                            return View();
                        }

                        // Уменьшаем количество товара на складе
                        stockItem.Count -= quantity;
                        _dbContext.Update(stockItem);

                        // Добавляем товар в продажу
                        sale.Saleitems.Add(new Saleitem
                        {
                            Id = random.Next(100000,999999).ToString(),
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

        // POST: Sales/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Получаем ID статуса "Отменена"
            int canceledStatusId = 3;

            // Возвращаем товары на склад
            foreach (var saleItem in sale.Saleitems)
            {
                var stockItem = await _dbContext.Products.FirstOrDefaultAsync(s => s.Id == saleItem.Productid);
                if (stockItem != null)
                {
                    stockItem.Count += saleItem.Count;
                    _dbContext.Update(stockItem);
                }
                // Если товара нет на складе, это странная ситуация, но можно ее обработать
            }

            // Изменяем статус продажи на "Отменена"
            sale.StatusId = canceledStatusId;
            _dbContext.Update(sale);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Обрабатываем ошибки при сохранении
                return StatusCode(500, "Ошибка при отмене продажи.");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Sales/Hide
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                // Обрабатываем ошибки
                return StatusCode(500, "Ошибка при изменении статуса продажи.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SaleExists(string id)
        {
            return _dbContext.Sales.Any(e => e.Id == id);
        }
    }
}

