using SpendingTracker.Models;

namespace SpendingTracker.Models.ViewModels;

public class DashboardViewModel
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyBudget { get; set; }
    public decimal BudgetUsedPercentage { get; set; }

    public List<Transaction> RecentTransactions { get; set; }
    public List<CategorySpending> CategorySpendings { get; set; }
    public List<MonthlyTrend> MonthlyTrends { get; set; }
}
public class CategorySpending
{
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public string Color { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyTrend
{
    public string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}