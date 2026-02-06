namespace Smartspendingtracker.Models;

public class DashboardViewModel
{
    public decimal TotalSpendingThisMonth { get; set; }
    public Dictionary<string, decimal> SpendingByCategory { get; set; } = new();
    public Dictionary<string, decimal> SpendingByCurrency { get; set; } = new();
    public List<ExpenseListItemViewModel> LatestExpenses { get; set; } = new();
    public int CurrentYear { get; set; }
    public int CurrentMonth { get; set; }
    public string CurrentMonthName { get; set; } = string.Empty;
}