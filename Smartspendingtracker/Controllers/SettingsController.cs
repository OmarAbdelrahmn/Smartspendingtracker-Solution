using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartspendingtracker.Models;
using SpendingTracker.Models;

namespace SpendingTracker.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings
                settings = new UserSettings
                {
                    PreferredCurrency = "SAR",
                    ExchangeRate = 13.5m
                };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserSettings model)
        {
            if (ModelState.IsValid)
            {
                var settings = await _context.UserSettings.FirstOrDefaultAsync();

                if (settings == null)
                {
                    _context.UserSettings.Add(model);
                }
                else
                {
                    settings.PreferredCurrency = model.PreferredCurrency;
                    settings.ExchangeRate = model.ExchangeRate;
                    settings.LastModified = DateTime.Now;
                    _context.Update(settings);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View("Index", model);
        }
    }
}