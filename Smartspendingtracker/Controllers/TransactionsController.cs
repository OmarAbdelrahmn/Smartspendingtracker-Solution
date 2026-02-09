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
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Description.Contains(search) || t.Notes.Contains(search));

            var transactions = await query
                .OrderByDescending(t => t.Date)
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
            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    Description = viewModel.Description,
                    Amount = viewModel.Amount,
                    Date = viewModel.Date,
                    CategoryId = viewModel.CategoryId,
                    Notes = viewModel.Notes,
                    PaymentMethod = viewModel.PaymentMethod,
                    IsRecurring = viewModel.IsRecurring
                };

                // Handle receipt image upload
                if (viewModel.ReceiptImage != null && viewModel.ReceiptImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ReceiptImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ReceiptImage.CopyToAsync(fileStream);
                    }

                    transaction.ReceiptImagePath = "/uploads/receipts/" + uniqueFileName;
                }

                _context.Add(transaction);
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
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                Notes = transaction.Notes,
                PaymentMethod = transaction.PaymentMethod,
                IsRecurring = transaction.IsRecurring,
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
                    transaction.Date = viewModel.Date;
                    transaction.CategoryId = viewModel.CategoryId;
                    transaction.Notes = viewModel.Notes;
                    transaction.PaymentMethod = viewModel.PaymentMethod;
                    transaction.IsRecurring = viewModel.IsRecurring;

                    // Handle new receipt image
                    if (viewModel.ReceiptImage != null && viewModel.ReceiptImage.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(transaction.ReceiptImagePath))
                        {
                            var oldPath = Path.Combine(_environment.WebRootPath,
                                transaction.ReceiptImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ReceiptImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ReceiptImage.CopyToAsync(fileStream);
                        }

                        transaction.ReceiptImagePath = "/uploads/receipts/" + uniqueFileName;
                    }

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
                // Delete receipt image if exists
                if (!string.IsNullOrEmpty(transaction.ReceiptImagePath))
                {
                    var path = Path.Combine(_environment.WebRootPath,
                        transaction.ReceiptImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

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