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

        [Required]
        public int CategoryId { get; set; }

        public string Notes { get; set; }

        public string PaymentMethod { get; set; }

        public bool IsRecurring { get; set; }

        public IFormFile ReceiptImage { get; set; }

        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> PaymentMethods { get; set; } = new()
        {
            new SelectListItem { Value = "Cash", Text = "Cash" },
            new SelectListItem { Value = "Credit Card", Text = "Credit Card" },
            new SelectListItem { Value = "Debit Card", Text = "Debit Card" },
            new SelectListItem { Value = "Bank Transfer", Text = "Bank Transfer" },
            new SelectListItem { Value = "Mobile Payment", Text = "Mobile Payment" }
        };
    }
}