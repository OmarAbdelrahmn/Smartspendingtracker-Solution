using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;

namespace SpendingTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // Calculate totals
            var totalSpent = await _context.Transactions.SumAsync(t => t.Amount);
            var monthlySpent = await _context.Transactions
                .Where(t => t.Date >= firstDayOfMonth)
                .SumAsync(t => t.Amount);

            // Recent transactions
            var recentTransactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalSpent = totalSpent;
            ViewBag.MonthlySpent = monthlySpent;
            ViewBag.RecentTransactions = recentTransactions;

            return View();
        }
    }
}