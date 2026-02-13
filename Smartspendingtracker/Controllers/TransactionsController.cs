using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpendingTracker.Models;
using SpendingTracker.Models.ViewModels;

namespace SpendingTracker.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TransactionsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? categoryId, string search)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);



            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;

            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new TransactionViewModel
            {
                Categories = await GetCategoriesSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionViewModel viewModel)
        {
            if (viewModel.Date == default)
                viewModel.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    Description = viewModel.Description,
                    Amount = viewModel.Amount,
                    CategoryId = viewModel.CategoryId,
                    CreatedAt = viewModel.Date, // or DateTime.Now
                    IsRecurring = viewModel.IsRecurring,

                };

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await GetCategoriesSelectList();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var viewModel = new TransactionViewModel
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                CategoryId = transaction.CategoryId,
                Categories = await GetCategoriesSelectList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var transaction = await _context.Transactions.FindAsync(id);
                    if (transaction == null) return NotFound();

                    transaction.Description = viewModel.Description;
                    transaction.Amount = viewModel.Amount;
                    transaction.CategoryId = viewModel.CategoryId;
                    transaction.IsRecurring = viewModel.IsRecurring;

                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(viewModel.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await GetCategoriesSelectList();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {  
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        private async Task<List<SelectListItem>> GetCategoriesSelectList()
        {
            return await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Group = new SelectListGroup { Name = c.IsExpense ? "Expenses" : "Income" }
                })
                .ToListAsync();
        }
    }
}