using System.Xml.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Globalization;

namespace WareHouse.Controllers
{
    [Authorize]
    public class ShipmentsController : Controller
    {
        private readonly NeondbContext _DbContext;
        private readonly Random _random;

        public ShipmentsController(NeondbContext dbContext)
        {
            _DbContext = dbContext;
            _random = new Random();


        }

        [Authorize] // Доступ для просмотра: Admin, ProcurementManager, WarehouseWorker, Accountant
        public async Task<IActionResult> Index(bool showHidden = false)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var shipments = await _DbContext.Shipments
                .Include(s => s.Status)
                .Include(s => s.User)
                .Include(s => s.Shipmentitems)
                .Where(s => s.isHidden == showHidden)
                .ToListAsync();
            ViewBag.Products = _DbContext.Products.ToList();
            ViewBag.Statuses = _DbContext.Statuses.ToList();
            ViewBag.Users = _DbContext.Users.ToList();
            ViewBag.ShowHidden = showHidden;
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
        [Authorize(Policy = "AdminOrProcurement")] // Создание поставок: Admin, ProcurementManager

        public async Task<IActionResult> CreateShipment(string[] ProductIds, Dictionary<string, string> Quantities)
        {
            if (Quantities == null || !Quantities.Any(q => double.Parse(q.Value, CultureInfo.InvariantCulture) > 0))
            {
                ModelState.AddModelError("", "Должен быть выбран хотя бы один товар");
                return BadRequest(ModelState);
            }
            if (ModelState.IsValid)
            {
                var shipment = new Shipment
                {
                    Id = _random.Next(10000,999999).ToString(),
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Statusid = 1,
                    Userid = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                _DbContext.Shipments.Add(shipment);
                _DbContext.SaveChanges();
                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && double.Parse(Quantities[productId], CultureInfo.InvariantCulture) > 0)
                    {
                        double quantity = double.Parse(Quantities[productId], CultureInfo.InvariantCulture);

                        _DbContext.Shipmentitems.Add(new Shipmentitem
                        {
                            Id = _random.Next(100000,999999).ToString(),
                            Productid = productId,
                            Shipmentid = shipment.Id,
                            Weight = quantity 
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
        [Authorize(Policy = "AdminOrProcurement")] // Редактирование поставок: Admin, ProcurementManager
        public async Task<IActionResult> EditShipment(string Id, string[] ProductIds, Dictionary<string, string> Quantities)
        {
            if (ModelState.IsValid)
            {
                // 1. Находим поставку в базе данных
                var shipment = await _DbContext.Shipments
                    .Include(s => s.Shipmentitems) 
                    .FirstOrDefaultAsync(s => s.Id == Id);

                if (shipment == null)
                {
                    return NotFound(); 
                }

                // 2. Обновляем основные данные поставки
                shipment.Userid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 3. Обновляем список товаров в поставке

                // 3.1. Удаляем существующие ShipmentItems (если нужно)
                _DbContext.Shipmentitems.RemoveRange(shipment.Shipmentitems);

                // 3.2. Добавляем новые ShipmentItems (или обновляем существующие)
                foreach (var productId in ProductIds)
                {
                    if (Quantities.ContainsKey(productId) && double.Parse(Quantities[productId], CultureInfo.InvariantCulture) > 0)
                    {
                        double quantity = double.Parse(Quantities[productId], CultureInfo.InvariantCulture);

                        shipment.Shipmentitems.Add(new Shipmentitem
                        {
                            Id = _random.Next(100000,999999).ToString(),
                            Productid = productId,
                            Shipmentid = shipment.Id,
                            Weight = quantity
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

           
            return View("Index"); // Возвращаемся на Index с заполненными данными
        }

        [Authorize(Policy = "AdminOrProcurement")] // Отмена поставок: Admin, ProcurementManager
        public IActionResult CancelShipment(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shipment = _DbContext.Shipments.FirstOrDefault(s => s.Id.Trim() == id.Trim());
            if (shipment == null)
            {
                return NotFound();
            }

            int canceledStatusId = 3;

            // Обновляем статус поставки на "Отменено"
            shipment.Statusid = canceledStatusId;
            _DbContext.Update(shipment);

            try
            {
                _DbContext.SaveChanges();
            }
            catch (Exception)
            {
                // Обработка ошибок при обновлении
                return StatusCode(500, "Ошибка при отмене поставки.");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOrProcurementOrWareHouseWorker")] // Подтверждение разгрузки: Admin, ProcurementManager, WarehouseWorker
        public IActionResult MarkAsArrived(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shipment = _DbContext.Shipments.FirstOrDefault(s => s.Id.Trim() == id.Trim());
            if (shipment == null)
            {
                return NotFound();
            }

            int arrivedStatusId = 2;

            shipment.Statusid = arrivedStatusId;
            _DbContext.Update(shipment);

            try
            {
                _DbContext.SaveChanges();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при изменении статуса поставки.");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOnly")] 
        public IActionResult DeleteShipment(string id)
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
            try
            {
                shipment.isHidden = true;
                shipment.Statusid = 6;
                _DbContext.SaveChanges();
            }
            catch (Exception)
            {
                //  Обработка ошибок при удалении, например, если есть связанные данные
                throw new Exception("Ошибка удаления");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminOrProcurementOrWareHouseWorker")] // Подтверждение разгрузки: Admin, ProcurementManager, WarehouseWorker
        public async Task<IActionResult> ConfirmUnloading(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var shipment = await _DbContext.Shipments
                .Include(s => s.Shipmentitems)
                    .ThenInclude(si => si.Product) 
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null)
            {
                return NotFound();
            }

            int unloadedStatusId = 4; 

            // Проверяем, что поставка еще не разгружена
            if (shipment.Statusid == unloadedStatusId)
            {
                ModelState.AddModelError("", "Поставка уже разгружена.");
                return View("Index");  //  Возвращаемся к списку с сообщением об ошибке
            }

            // Начинаем транзакцию для обеспечения целостности данных
            using (var transaction = await _DbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Обновляем статус поставки на "Разгружено"
                    shipment.Statusid = unloadedStatusId;
                    _DbContext.Update(shipment);

                    // 2. Добавляем товары на склад (или обновляем их количество)
                    foreach (var shipmentItem in shipment.Shipmentitems)
                    {
                        var product = shipmentItem.Product; 
                        if (product != null)
                        {
                            // Получаем существующую запись о товаре на складе (если есть)
                            var stockItem = await _DbContext.Products
                                .FirstOrDefaultAsync(s => s.Id == product.Id);

                            // Если товар уже есть на складе, увеличиваем его количество
                            stockItem.Weight += shipmentItem.Weight;
                            _DbContext.Update(stockItem);
                        }
                    }

                    // Сохраняем изменения в базе данных
                    await _DbContext.SaveChangesAsync();

                    // Подтверждаем транзакцию
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Откатываем транзакцию в случае ошибки
                    await transaction.RollbackAsync();

                    ModelState.AddModelError("", "Ошибка при подтверждении разгрузки: " + ex.Message);
                    return View("Index");  //  Возвращаемся к списку с сообщением об ошибке
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ShipmentExists(string id)
        {
            return _DbContext.Shipments.Any(e => e.Id == id);
        }


    }
}
