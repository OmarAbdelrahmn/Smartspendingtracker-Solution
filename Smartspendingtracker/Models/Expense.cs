using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smartspendingtracker.Models;

public class Expense
{
        [Key]
    public int Id { get; set; }

    /// <summary>
    /// Original amount in the transaction currency
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency of the transaction
    /// </summary>
    [Required]
    public Currency Currency { get; set; }

    /// <summary>
    /// Converted amount in base currency (EGP)
    /// This is calculated and stored for fast queries
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ConvertedAmountInEGP { get; set; }

    /// <summary>
    /// Category ID (foreign key)
    /// </summary>
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Navigation property to Category
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Original description/text from user
    /// For chat: the raw input text
    /// For manual: user-entered description
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the expense was created (UTC+3)
    /// </summary>
    [Required]
    public DateTime DateTime { get; set; }

    /// <summary>
    /// How this expense was created
    /// </summary>
    [Required]
    public ExpenseSource Source { get; set; }

    /// <summary>
    /// Exchange rate used for conversion (if applicable)
    /// Null if currency = EGP
    /// </summary>
    [Column(TypeName = "decimal(18,6)")]
    public decimal? ExchangeRateUsed { get; set; }
}