using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smartspendingtracker.Models
{
    public class UserSettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        public string PreferredCurrency { get; set; } = "SAR"; // SAR or EGP

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRate { get; set; } = 12.44m; // 1 SAR = X EGP (default ~13.5)

        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}