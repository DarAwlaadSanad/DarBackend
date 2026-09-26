using DarV2.Enum;
using System.ComponentModel.DataAnnotations;

namespace DarV2.DTOs.Finance
{
    public class UserContractDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal Amount { get; set; }
    }

    public class SetUserContractDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public SalaryType SalaryType { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }

    public class FinancialSettingDTO
    {
        public decimal DelayDeductionAmount { get; set; }
        public decimal AbsenceSessionDeduction { get; set; }
    }

    public class FinancialTransactionDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Reason { get; set; }
    }

    public class AddFinancialTransactionDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? Reason { get; set; }
    }

    public class MonthlyPayrollReportDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public SalaryType SalaryType { get; set; }
        public decimal BaseSalary { get; set; }
        
        // Detailed stats
        public int NumberOfGroups { get; set; }
        public int DelayIncidents { get; set; }
        public int Absences { get; set; }
        public int Substitutions { get; set; }

        // Financial Breakdown
        public decimal AutomaticDeductions { get; set; }
        public decimal AutomaticAdditions { get; set; }
        public decimal ManualDeductions { get; set; }
        public decimal ManualAdditions { get; set; }

        public decimal NetSalary { get; set; }
        public bool IsPaid { get; set; }
    }

    public class TogglePaymentDTO
    {
        public string UserId { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
