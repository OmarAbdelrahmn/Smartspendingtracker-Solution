using Microsoft.AspNetCore.Mvc;
using Smartspendingtracker.Models;
using SmartSpendingTracker.Services;

namespace SmartSpendingTracker.Controllers
{
    /// <summary>
    /// Main controller for the Smart Spending Tracker application
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ExpenseService _expenseService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ExpenseService expenseService,
            ILogger<HomeController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        /// <summary>
        /// Main dashboard page
        /// GET: /
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var dashboardData = await _expenseService.GetDashboardDataAsync();
            return View(dashboardData);
        }

        /// <summary>
        /// Process chat input and create expense
        /// POST: /Home/ProcessChat
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProcessChat([FromBody] ChatInputViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Message))
            {
                return Json(new ChatResponseViewModel
                {
                    Success = false,
                    Message = "Please enter a message"
                });
            }

            var (success, message, expense) = await _expenseService.CreateExpenseFromChatAsync(model.Message);

            var response = new ChatResponseViewModel
            {
                Success = success,
                Message = message
            };

            if (success && expense != null)
            {
                response.Expense = new ExpenseListItemViewModel
                {
                    Id = expense.Id,
                    Amount = expense.Amount,
                    Currency = expense.Currency.ToString(),
                    ConvertedAmountInEGP = expense.ConvertedAmountInEGP,
                    CategoryName = expense.Category.NameEnglish,
                    CategoryIcon = expense.Category.IconClass,
                    CategoryColor = expense.Category.Color,
                    Description = expense.Description,
                    DateTime = expense.DateTime,
                    Source = expense.Source.ToString(),
                    IsFromChat = true
                };
            }

            return Json(response);
        }

        /// <summary>
        /// Show create expense form
        /// GET: /Home/Create
        /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _expenseService.GetAllCategoriesAsync();
            return View(new CreateExpenseViewModel());
        }

        /// <summary>
        /// Create expense from form
        /// POST: /Home/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExpenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _expenseService.GetAllCategoriesAsync();
                return View(model);
            }

            // Parse currency
            Currency currency;
            if (!Enum.TryParse<Currency>(model.Currency, out currency))
            {
                currency = Currency.EGP;
            }

            var (success, message, expense) = await _expenseService.CreateExpenseManuallyAsync(
                model.Amount,
                currency,
                model.CategoryId,
                model.Description,
                model.Date
            );

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", message);
            ViewBag.Categories = await _expenseService.GetAllCategoriesAsync();
            return View(model);
        }

        /// <summary>
        /// Delete an expense
        /// POST: /Home/Delete/{id}
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _expenseService.DeleteExpenseAsync(id);

            if (success)
            {
                return Json(new { success = true, message = "Expense deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete expense" });
        }

        /// <summary>
        /// Get dashboard data as JSON (for AJAX refresh)
        /// GET: /Home/GetDashboardData
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? year, int? month)
        {
            DashboardViewModel dashboardData;

            if (year.HasValue && month.HasValue)
            {
                dashboardData = await _expenseService.GetDashboardDataAsync(year.Value, month.Value);
            }
            else
            {
                dashboardData = await _expenseService.GetDashboardDataAsync();
            }

            return Json(dashboardData);
        }

        /// <summary>
        /// Error page
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}