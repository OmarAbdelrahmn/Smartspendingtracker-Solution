using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;
using SpendingTracker.Models.ViewModels;

namespace SpendingTracker.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? categoryId)
        {
            var today = DateTime.Now;
            startDate ??= new DateTime(today.Year, today.Month, 1);
            endDate ??= today;

            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);

            var transactions = await query.OrderBy(t => t.CreatedAt).ToListAsync();

            var totalIncome = transactions.Where(t => !t.Category.IsExpense).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Category.IsExpense).Sum(t => t.Amount);

            // Category summaries
            var categorySummaries = transactions
                .GroupBy(t => new { t.Category.Name, t.Category.Color, t.Category.IsExpense })
                .Select(g => new CategorySummary
                {
                    CategoryName = g.Key.Name,
                    Color = g.Key.Color,
                    Amount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    PercentageOfTotal = g.Key.IsExpense
                        ? (totalExpense > 0 ? (double)(g.Sum(t => t.Amount) / totalExpense) * 100 : 0)
                        : (totalIncome > 0 ? (double)(g.Sum(t => t.Amount) / totalIncome) * 100 : 0)
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            // Daily summaries
            var dailySummaries = transactions
                .GroupBy(t => t.CreatedAt)
                .Select(g => new DailySummary
                {
                    Date = g.Key,
                    Income = g.Where(t => !t.Category.IsExpense).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Category.IsExpense).Sum(t => t.Amount)
                })
                .ToList();

            var viewModel = new ReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                CategoryId = categoryId,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetSavings = totalIncome - totalExpense,
                Transactions = transactions,
                CategorySummaries = categorySummaries,
                DailySummaries = dailySummaries
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Charts()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(int year)
        {
            var monthlyData = new List<object>();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var income = await _context.Transactions
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate && !t.Category.IsExpense)
                    .SumAsync(t => t.Amount);

                var expense = await _context.Transactions
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate && t.Category.IsExpense)
                    .SumAsync(t => t.Amount);

                monthlyData.Add(new
                {
                    month = startDate.ToString("MMM"),
                    income,
                    expense
                });
            }

            return Json(monthlyData);
        }
    }
}