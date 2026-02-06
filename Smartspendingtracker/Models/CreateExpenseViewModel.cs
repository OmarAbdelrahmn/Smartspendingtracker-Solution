using System.ComponentModel.DataAnnotations;

namespace Smartspendingtracker.Models
{
    /// <summary>
    /// ViewModel for creating expense via traditional form
    /// </summary>
    public class CreateExpenseViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999999.99")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required")]
        public string Currency { get; set; } = "EGP";

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}