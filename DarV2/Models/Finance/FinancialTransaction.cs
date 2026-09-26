using DarV2.Enum;
using DarV2.Models;
using System.ComponentModel.DataAnnotations;

namespace DarV2.Models.Finance
{
    public class FinancialTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public decimal Amount { get; set; } // Positive for bonus, negative for deduction

        public TransactionType Type { get; set; }

        public DateTime TransactionDate { get; set; }

        public string? Reason { get; set; }
    }
}
