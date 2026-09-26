using DarV2.Enum;
using DarV2.Models;
using System.ComponentModel.DataAnnotations;

namespace DarV2.Models.Finance
{
    public class UserContract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public SalaryType SalaryType { get; set; }

        public decimal Amount { get; set; }
    }
}
