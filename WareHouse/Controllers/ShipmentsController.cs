using System.Xml.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WareHouse.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly NeondbContext _DbContext;
        private readonly Random _random;

        public ShipmentsController(NeondbContext dbContext)
        {
            _DbContext = dbContext;
            _random = new Random();
        }

        public async Task<IActionResult> Index()
        {
            // Загружаем поставки, статусы и пользователей (вместо ViewBags лучше использовать ViewModel)
            var shipments = await _DbContext.Shipments
                .Include(s => s.Status) // Подгружаем статус
                .Include(s => s.User) 
                .Include(s => s.Shipmentitems)
                .Where(s => s.isHidden == false)// Подгружаем пользователя
                .ToListAsync();
            ViewBag.Products = _DbContext.Products.ToList();
            ViewBag.Statuses = _DbContext.Statuses.ToList();
            ViewBag.Users = _DbContext.Users.ToList();  
            // Передаем данные в представление через ViewBag.
            // ...
            Dictionary<string, string> shipmentItemsJson = new Dictionary<string, string>();

            foreach (var shipment in shipments)
            {
                // Сериализуем Shipmentitems в JSON
                string json = JsonConvert.SerializeObject(shipment.Shipmentitems, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                // Сохраняем JSON в словаре
                shipmentItemsJson[shipment.Id] = json;
            }

            // Передаем список shipments и словарь с JSON-представлениями в ViewBag
            ViewBag.Shipments = shipments;
            ViewBag.ShipmentItemsJson = shipmentItemsJson;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShipment(string[] ProductIds, Dictionary<string, int> Quantities)
        {
            if (ModelState.IsValid)
            {
                var shipment = new Shipment
                {
                    Id = _random.Next(10000,999999).ToString(),
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Statusid = 1,
                    Userid = "668859"
                };
                _DbContext.Shipments.Add(shipment);
                _DbContext.SaveChanges();
                // Добавляем ShipmentItems
                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && Quantities[productId] > 0)
                    {
                        int quantity = Quantities[productId];

                        _DbContext.Shipmentitems.Add(new Shipmentitem
                        {
                            Id = _random.Next(100000,999999).ToString(),
                            Productid = productId,
                            Shipmentid = shipment.Id,
                            Count = quantity // Используем указанное количество
                        });
                    }
                }
                _DbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Statuses = await _DbContext.Statuses.ToListAsync();
                ViewBag.Users = await _DbContext.Users.ToListAsync();
                ViewBag.Products = await _DbContext.Products.ToListAsync();
                return View();
            }
        }


        // POST: Shipments/EditShipment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShipment(string Id, int StatusId, string UserId, string[] ProductIds, Dictionary<string, int> Quantities)
        {
            if (ModelState.IsValid)
            {
                // 1. Находим поставку в базе данных
                var shipment = await _DbContext.Shipments
                    .Include(s => s.Shipmentitems) // Важно, чтобы подгрузить ShipmentItems
                    .FirstOrDefaultAsync(s => s.Id == Id);

                if (shipment == null)
                {
                    return NotFound(); // Или другой подходящий ответ
                }

                // 2. Обновляем основные данные поставки
                shipment.Statusid = StatusId;
                shipment.Userid = UserId;

                // 3. Обновляем список товаров в поставке

                // 3.1. Удаляем существующие ShipmentItems (если нужно)
                // Это безопасно, т.к. мы пересоздаем список
                _DbContext.Shipmentitems.RemoveRange(shipment.Shipmentitems);

                // 3.2. Добавляем новые ShipmentItems (или обновляем существующие)
                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && Quantities[productId] > 0)
                    {
                        int quantity = Quantities[productId];

                        shipment.Shipmentitems.Add(new Shipmentitem
                        {
                            Id = _random.Next(100000,999999).ToString(),
                            Productid = productId,
                            Shipmentid = shipment.Id,
                            Count = quantity
                        });
                    }
                }

                // 4. Сохраняем изменения в базе данных
                try
                {
                    _DbContext.Update(shipment);
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Обработка конфликтов конкурентного доступа
                    if (!ShipmentExists(shipment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    //  Обработка других ошибок при сохранении
                    ModelState.AddModelError("", "Ошибка при сохранении изменений: " + ex.Message);
                    return View("Index");  //  Или возвращаем частичное представление с модальным окном и ошибками
                }

                return RedirectToAction(nameof(Index)); // Перенаправляем на список поставок
            }

            // Если модель не прошла валидацию, возвращаем представление с ошибками
            // (В реальном приложении лучше использовать Ajax для сохранения и отображать ошибки в модальном окне)
            return View("Index"); // Возвращаемся на Index с заполненными данными
        }

        // POST: Shipments/DeleteShipment
        public IActionResult HideShipment(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipment = _DbContext.Shipments.FirstOrDefault(s => s.Id.Trim() == id.Trim());
            if (shipment == null)
            {
                return NotFound();
            }
            if (shipment.Statusid != 3) 
            {
                throw new Exception("Поставка должна быть отменена перед удалением");
            }
            try
            {
                shipment.isHidden = true;
                _DbContext.SaveChanges();
            }
            catch (Exception)
            {
                //  Обработка ошибок при удалении, например, если есть связанные данные
                throw new Exception("Ошибка удаления");
            }

            return RedirectToAction("Index");
        }

        private bool ShipmentExists(string id)
        {
            return _DbContext.Shipments.Any(e => e.Id == id);
        }


    }
}
