using System.ComponentModel.DataAnnotations;

namespace Smartspendingtracker.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NameEnglish { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Keywords for automatic detection (comma-separated, Arabic & English)
    /// Example: "أكل,مطعم,قهوة,food,restaurant,coffee"
    /// </summary>
    [Required]
    public string Keywords { get; set; } = string.Empty;

    /// <summary>
    /// Icon class for UI (e.g., "fa-utensils", "fa-car")
    /// </summary>
    [MaxLength(50)]
    public string IconClass { get; set; } = "fa-folder";

    /// <summary>
    /// Color for charts and UI
    /// </summary>
    [MaxLength(20)]
    public string Color { get; set; } = "#6c757d";

    // Navigation property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}