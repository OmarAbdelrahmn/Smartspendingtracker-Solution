namespace Smartspendingtracker.Models;

public class ChatResponseViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExpenseListItemViewModel? Expense { get; set; }
}
