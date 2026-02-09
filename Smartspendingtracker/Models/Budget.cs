using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendingTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; } // Null means overall budget
        public virtual Category Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}