using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Color { get; set; } = "#3498db";

        [StringLength(50)]
        public string Icon { get; set; } = "fa-tag";

        public bool IsExpense { get; set; } = true;

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}