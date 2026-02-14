using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;

namespace SpendingTracker.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // ✅ ADD THIS at the beginning
            var settings = await _context.UserSettings.FirstOrDefaultAsync();

            var today = DateTime.Now;
            startDate ??= new DateTime(today.Year, today.Month, 1);
            endDate ??= today;

            var transactions = await _context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderBy(t => t.Date)
                .ToListAsync();

            var totalSpent = transactions.Sum(t => t.Amount);

            // Daily summary
            var dailySummary = transactions
                .GroupBy(t => t.Date.Date)
                .Select(g => new DailySummary
                {
                    Date = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Top spending locations
            var topLocations = transactions
                .GroupBy(t => t.Description)
                .Select(g => new LocationSummary
                {
                    Location = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(l => l.Amount)
                .Take(10)
                .ToList();

            // ✅ ADD THIS
            ViewBag.Settings = settings;
            ViewBag.StartDate = startDate.Value;
            ViewBag.EndDate = endDate.Value;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.DailySummary = dailySummary;
            ViewBag.TopLocations = topLocations;
            ViewBag.Transactions = transactions;

            return View();
        }
    }

    public class DailySummary
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class LocationSummary
    {
        public string Location { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}