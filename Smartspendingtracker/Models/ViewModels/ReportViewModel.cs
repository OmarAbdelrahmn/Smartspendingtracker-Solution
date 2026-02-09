namespace SpendingTracker.Models.ViewModels
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? CategoryId { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetSavings { get; set; }

        public List<Transaction> Transactions { get; set; }
        public List<CategorySummary> CategorySummaries { get; set; }
        public List<DailySummary> DailySummaries { get; set; }
    }

    public class CategorySummary
    {
        public string CategoryName { get; set; }
        public string Color { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
        public double PercentageOfTotal { get; set; }
    }

    public class DailySummary
    {
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}