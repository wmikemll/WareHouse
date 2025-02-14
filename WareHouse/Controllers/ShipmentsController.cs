using System.Xml.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

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
                .Include(s => s.User) // Подгружаем пользователя
                .ToListAsync();

            // Получаем статусы и пользователей для выпадающих списков
            var statuses = await _DbContext.Statuses.ToListAsync();
            var users = await _DbContext.Users.ToListAsync();  // Предполагаем, что есть таблица Users

            // Передаем данные в представление через ViewBag.
            ViewBag.Shipments = shipments;
            ViewBag.Statuses = statuses;
            ViewBag.Users = users;

            return View();
        }

        // POST: Shipments/CreateShipment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShipment(string EmployeeId)
        {
            _DbContext.Shipments.Add(new Shipment
            {
                Id = _random.Next(100000, 999999).ToString(),
                Date = DateOnly.FromDateTime(DateTime.Now),
                Userid = EmployeeId,
                Statusid = _DbContext.Statuses.FirstOrDefault(s => s.Name.Trim() == "Отправлена").Id
            });
            _DbContext.SaveChanges();
            return RedirectToAction("Index");

        }

        // POST: Shipments/EditShipment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShipment(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _DbContext.Update(shipment); // Обновляем запись в базе данных
                    await _DbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Обработка конфликтов при одновременном изменении
                    if (!ShipmentExists(shipment.Id))
                    {
                        return NotFound(); // Если поставка не найдена
                    }
                    else
                    {
                        throw; //  Повторно выбрасываем исключение, чтобы обработать его выше
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Если модель не валидна, возвращаем представление с ошибками
            ViewBag.Shipments = await _DbContext.Shipments.ToListAsync();
            ViewBag.Statuses = await _DbContext.Statuses.ToListAsync();
            ViewBag.Users = await _DbContext.Users.ToListAsync();
            return View("Index");
        }

        // POST: Shipments/DeleteShipment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShipment(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipment = await _DbContext.Shipments.FindAsync(id);
            if (shipment == null)
            {
                return NotFound();
            }

            try
            {
                _DbContext.Shipments.Remove(shipment);
                await _DbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                //  Обработка ошибок при удалении, например, если есть связанные данные
                return RedirectToAction(nameof(Index)); //  Перенаправляем обратно в Index
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ShipmentExists(string id)
        {
            return _DbContext.Shipments.Any(e => e.Id == id);
        }


    }
}
