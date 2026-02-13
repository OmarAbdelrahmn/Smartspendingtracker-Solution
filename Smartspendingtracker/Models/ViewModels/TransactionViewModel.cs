using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.Models.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; }

        public bool IsRecurring { get; set; }

        public IFormFile ReceiptImage { get; set; }

        public List<SelectListItem> Categories { get; set; }
    }
}