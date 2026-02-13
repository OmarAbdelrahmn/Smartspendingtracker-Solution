using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;
using SpendingTracker.Models.ViewModels;

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
            var monthlyIncome = await _context.Transactions
                .Where(t =>  !t.Category.IsExpense)
                .SumAsync(t => t.Amount);

            var monthlyExpense = await _context.Transactions
                .Where(t => t.Category.IsExpense)
                .SumAsync(t => t.Amount);

            var totalBalance = await _context.Transactions
                .SumAsync(t => t.Category.IsExpense ? -t.Amount : t.Amount);

            // Get budget for current month
            var monthlyBudget = await _context.Budgets
                .Where(b => b.Month == today.Month && b.Year == today.Year && b.CategoryId == null)
                .Select(b => b.Amount)
                .FirstOrDefaultAsync();

            var budgetUsedPercentage = monthlyBudget > 0
                ? (monthlyExpense / monthlyBudget) * 100
                : 0;

            // Recent transactions
            var recentTransactions = await _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Category spending for current month
            var categorySpendings = await _context.Transactions
                .Where(t => t.CreatedAt >= firstDayOfMonth && t.Category.IsExpense)
                .GroupBy(t => new { t.Category.Name, t.Category.Color })
                .Select(g => new CategorySpending
                {
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(t => t.Amount),
                    Color = g.Key.Color,
                    Percentage = monthlyExpense > 0 ? (double)(g.Sum(t => t.Amount) / monthlyExpense) * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToListAsync();

            // Monthly trend (last 6 months)
            var monthlyTrends = new List<MonthlyTrend>();
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var income = await _context.Transactions
                    .Where(t => t.CreatedAt >= monthStart && t.CreatedAt <= monthEnd && !t.Category.IsExpense)
                    .SumAsync(t => t.Amount);

                var expense = await _context.Transactions
                    .Where(t => t.CreatedAt >= monthStart && t.CreatedAt <= monthEnd && t.Category.IsExpense)
                    .SumAsync(t => t.Amount);

                monthlyTrends.Add(new MonthlyTrend
                {
                    Month = month.ToString("MMM yyyy"),
                    Income = income,
                    Expense = expense
                });
            }

            var viewModel = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                MonthlyIncome = monthlyIncome,
                MonthlyExpense = monthlyExpense,
                MonthlyBudget = monthlyBudget,
                BudgetUsedPercentage = budgetUsedPercentage,
                RecentTransactions = recentTransactions,
                CategorySpendings = categorySpendings,
                MonthlyTrends = monthlyTrends
            };

            return View(viewModel);
        }
    }
}