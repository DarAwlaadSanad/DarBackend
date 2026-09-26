using System.ComponentModel.DataAnnotations;

namespace DarV2.Models.Finance
{
    public class FinancialSetting
    {
        [Key]
        public int Id { get; set; }

        public decimal DelayDeductionAmount { get; set; } // Fixed deduction amount per delay incident
        public decimal AbsenceSessionDeduction { get; set; } // Deduction amount per absent session
    }
}
