namespace Smartspendingtracker.Models;

public class ExpenseListItemViewModel
{

    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal ConvertedAmountInEGP { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsFromChat { get; set; }
}