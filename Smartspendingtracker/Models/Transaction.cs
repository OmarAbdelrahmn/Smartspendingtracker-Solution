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

        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public bool IsRecurring { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}