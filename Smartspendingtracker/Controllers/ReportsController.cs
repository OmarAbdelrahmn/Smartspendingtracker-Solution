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

            var recordedDays = transactions.Select(t => t.Date.Date).Distinct().Count();

            // Daily summary
            // Daily summary (INCLUDING ZERO DAYS)

            var grouped = transactions
                .GroupBy(t => t.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Amount = g.Sum(t => t.Amount),
                        Count = g.Count()
                    });

            var dailySummary = new List<DailySummary>();

            for (var date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
            {
                if (grouped.ContainsKey(date))
                {
                    dailySummary.Add(new DailySummary
                    {
                        Date = date,
                        Amount = grouped[date].Amount,
                        Count = grouped[date].Count
                    });
                }
                else
                {
                    dailySummary.Add(new DailySummary
                    {
                        Date = date,
                        Amount = 0,
                        Count = 0
                    });
                }
            }

            // Remove leading zero days (before first real data)
            var firstDayWithData = dailySummary.FirstOrDefault(d => d.Amount > 0);

            if (firstDayWithData != null)
            {
                dailySummary = dailySummary
                    .SkipWhile(d => d.Amount == 0)
                    .ToList();
            }

            // Average per day from first day with data to endDate
            int daysCount = (int)(endDate.Value.Date - dailySummary.First().Date.Date).TotalDays + 1;

            var averagePerDay = daysCount > 0 ? dailySummary.Sum(d => d.Amount) / daysCount : 0;

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
            ViewBag.AveragePerDay = averagePerDay;  // ✅ ADD THIS LINE
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