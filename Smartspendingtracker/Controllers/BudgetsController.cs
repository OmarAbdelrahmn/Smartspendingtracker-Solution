using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;

namespace SpendingTracker.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var today = DateTime.Now;
            year ??= today.Year;
            month ??= today.Month;

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.Month == month && b.Year == year)
                .ToListAsync();

            // Calculate spending for each budget
            var budgetViewModels = new List<BudgetViewModel>();

            foreach (var budget in budgets)
            {
                var spent = await _context.Transactions
                    .Where(t => t.Date.Month == month && t.Date.Year == year
                        && (budget.CategoryId == null || t.CategoryId == budget.CategoryId)
                        && t.Category.IsExpense)
                    .SumAsync(t => t.Amount);

                budgetViewModels.Add(new BudgetViewModel
                {
                    Budget = budget,
                    Spent = spent,
                    Remaining = budget.Amount - spent,
                    PercentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
                });
            }

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.Categories = await _context.Categories.Where(c => c.IsExpense).ToListAsync();

            return View(budgetViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int? categoryId, decimal amount, int month, int year, string notes)
        {
            var budget = new Budget
            {
                CategoryId = categoryId,
                Amount = amount,
                Month = month,
                Year = year,
                Notes = notes
            };

            _context.Add(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { year, month });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, decimal amount, string notes)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            budget.Amount = amount;
            budget.Notes = notes;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { year = budget.Year, month = budget.Month });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { year = budget?.Year, month = budget?.Month });
        }
    }

    public class BudgetViewModel
    {
        public Budget Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
    }
}