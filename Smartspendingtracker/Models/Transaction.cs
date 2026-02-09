using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendingTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public string PaymentMethod { get; set; } // Cash, Credit Card, Debit Card, etc.

        public bool IsRecurring { get; set; }

        public string ReceiptImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}