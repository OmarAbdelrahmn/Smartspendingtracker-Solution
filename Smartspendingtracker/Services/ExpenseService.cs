using Microsoft.EntityFrameworkCore;
using Smartspendingtracker;
using Smartspendingtracker.Models;

namespace SmartSpendingTracker.Services
{
    /// <summary>
    /// Service for managing expenses - CRUD operations and analytics
    /// </summary>
    public class ExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyConversionService _currencyService;
        private readonly ChatParsingService _chatParsingService;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(
            ApplicationDbContext context,
            CurrencyConversionService currencyService,
            ChatParsingService chatParsingService,
            ILogger<ExpenseService> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _chatParsingService = chatParsingService;
            _logger = logger;
        }

        /// <summary>
        /// Create expense from chat input
        /// </summary>
        public async Task<(bool success, string message, Expense? expense)> CreateExpenseFromChatAsync(string chatInput)
        {
            try
            {
                // Parse the chat input
                var parsed = await _chatParsingService.ParseAsync(chatInput);

                if (!parsed.IsValid)
                {
                    return (false, parsed.ErrorMessage ?? "Failed to parse input", null);
                }

                // Find matching category
                var category = await FindCategoryByKeywordAsync(parsed.DetectedCategory);

                // Get current date/time in UTC+3
                var now = _currencyService.GetCurrentDateTime();

                // Convert to EGP
                var (convertedAmount, exchangeRate) = await _currencyService.ConvertToEGPAsync(
                    parsed.Amount,
                    parsed.Currency,
                    now.Year,
                    now.Month);

                // Create expense
                var expense = new Expense
                {
                    Amount = parsed.Amount,
                    Currency = parsed.Currency,
                    ConvertedAmountInEGP = convertedAmount,
                    CategoryId = category.Id,
                    Description = parsed.OriginalText,
                    DateTime = now,
                    Source = ExpenseSource.Chat,
                    ExchangeRateUsed = exchangeRate
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                // Load the category for the response
                await _context.Entry(expense).Reference(e => e.Category).LoadAsync();

                // Generate confirmation message
                var isArabic = _chatParsingService.IsArabicInput(chatInput);
                var categoryName = isArabic ? category.NameArabic : category.NameEnglish;
                var message = _chatParsingService.GenerateConfirmationMessage(
                    parsed.Amount,
                    parsed.Currency,
                    categoryName,
                    isArabic);

                _logger.LogInformation("Expense created from chat: {ExpenseId}, Amount: {Amount} {Currency}",
                    expense.Id, expense.Amount, expense.Currency);

                return (true, message, expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense from chat: {Input}", chatInput);
                return (false, "An error occurred while processing your request", null);
            }
        }

        /// <summary>
        /// Create expense manually (from form)
        /// </summary>
        public async Task<(bool success, string message, Expense? expense)> CreateExpenseManuallyAsync(
            decimal amount,
            Currency currency,
            int categoryId,
            string description,
            DateTime? dateTime = null)
        {
            try
            {
                // Use provided date/time or current
                var expenseDateTime = dateTime ?? _currencyService.GetCurrentDateTime();

                // Convert to EGP
                var (convertedAmount, exchangeRate) = await _currencyService.ConvertToEGPAsync(
                    amount,
                    currency,
                    expenseDateTime.Year,
                    expenseDateTime.Month);

                // Create expense
                var expense = new Expense
                {
                    Amount = amount,
                    Currency = currency,
                    ConvertedAmountInEGP = convertedAmount,
                    CategoryId = categoryId,
                    Description = description,
                    DateTime = expenseDateTime,
                    Source = ExpenseSource.Manual,
                    ExchangeRateUsed = exchangeRate
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                // Load the category
                await _context.Entry(expense).Reference(e => e.Category).LoadAsync();

                _logger.LogInformation("Expense created manually: {ExpenseId}, Amount: {Amount} {Currency}",
                    expense.Id, expense.Amount, expense.Currency);

                return (true, "Expense created successfully", expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense manually");
                return (false, "An error occurred while creating the expense", null);
            }
        }

        /// <summary>
        /// Find category by keyword or return "Other" category
        /// </summary>
        private async Task<Category> FindCategoryByKeywordAsync(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Return "Other" category as default
                return await GetOtherCategoryAsync();
            }

            // Search for category containing this keyword
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Keywords.ToLower().Contains(keyword.ToLower()));

            return category ?? await GetOtherCategoryAsync();
        }

        /// <summary>
        /// Get the "Other" category (fallback)
        /// </summary>
        private async Task<Category> GetOtherCategoryAsync()
        {
            var otherCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.NameEnglish == "Other");

            if (otherCategory == null)
            {
                // Create it if it doesn't exist
                otherCategory = new Category
                {
                    NameEnglish = "Other",
                    NameArabic = "أخرى",
                    Keywords = "other,أخرى,متفرقات",
                    IconClass = "fa-folder",
                    Color = "#6C757D"
                };
                _context.Categories.Add(otherCategory);
                await _context.SaveChangesAsync();
            }

            return otherCategory;
        }

        /// <summary>
        /// Get dashboard data for the current month
        /// </summary>
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var now = _currencyService.GetCurrentDateTime();
            return await GetDashboardDataAsync(now.Year, now.Month);
        }

        /// <summary>
        /// Get dashboard data for a specific month
        /// </summary>
        public async Task<DashboardViewModel> GetDashboardDataAsync(int year, int month)
        {
            // Get all expenses for the month
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.DateTime >= startDate && e.DateTime < endDate)
                .OrderByDescending(e => e.DateTime)
                .ToListAsync();

            // Calculate total spending
            var totalSpending = expenses.Sum(e => e.ConvertedAmountInEGP);

            // Spending by category
            var spendingByCategory = expenses
                .GroupBy(e => e.Category.NameEnglish)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.ConvertedAmountInEGP)
                );

            // Spending by currency
            var spendingByCurrency = expenses
                .GroupBy(e => e.Currency)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Sum(e => e.Amount)
                );

            // Latest expenses (top 10)
            var latestExpenses = expenses
                .Take(10)
                .Select(e => new ExpenseListItemViewModel
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Currency = e.Currency.ToString(),
                    ConvertedAmountInEGP = e.ConvertedAmountInEGP,
                    CategoryName = e.Category.NameEnglish,
                    CategoryIcon = e.Category.IconClass,
                    CategoryColor = e.Category.Color,
                    Description = e.Description,
                    DateTime = e.DateTime,
                    Source = e.Source.ToString(),
                    IsFromChat = e.Source == ExpenseSource.Chat
                })
                .ToList();

            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June",
                                    "July", "August", "September", "October", "November", "December" };

            return new DashboardViewModel
            {
                TotalSpendingThisMonth = totalSpending,
                SpendingByCategory = spendingByCategory,
                SpendingByCurrency = spendingByCurrency,
                LatestExpenses = latestExpenses,
                CurrentYear = year,
                CurrentMonth = month,
                CurrentMonthName = monthNames[month]
            };
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.NameEnglish).ToListAsync();
        }

        /// <summary>
        /// Delete an expense
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int id)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                    return false;

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Expense deleted: {ExpenseId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense: {ExpenseId}", id);
                return false;
            }
        }
    }
}