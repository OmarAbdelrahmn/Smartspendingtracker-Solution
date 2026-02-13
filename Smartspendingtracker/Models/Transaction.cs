using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendingTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } // Where money was spent

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}