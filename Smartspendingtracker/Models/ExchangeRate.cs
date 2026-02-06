using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smartspendingtracker.Models;

public class ExchangeRate
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Source currency
    /// </summary>
    [Required]
    public Currency FromCurrency { get; set; }

    /// <summary>
    /// Target currency (always EGP in our case)
    /// </summary>
    [Required]
    public Currency ToCurrency { get; set; }

    /// <summary>
    /// Exchange rate value
    /// Example: SAR to EGP = 13.5 (1 SAR = 13.5 EGP)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal Rate { get; set; }

    /// <summary>
    /// Year for this rate
    /// </summary>
    [Required]
    public int Year { get; set; }

    /// <summary>
    /// Month for this rate (1-12)
    /// </summary>
    [Required]
    public int Month { get; set; }

    /// <summary>
    /// When this rate was created/updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}