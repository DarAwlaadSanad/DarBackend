using System;
using System.Collections.Generic;

namespace DarV2.DTOs.Finance
{
    public class CenterExpenseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateCenterExpenseDTO
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }

    public class CenterIncomeDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateCenterIncomeDTO
    {
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }

    public class MonthlyFinancialSummaryDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        
        public decimal TotalStudentFees { get; set; }
        public decimal TotalOtherIncomes { get; set; }
        public decimal TotalIncomes => TotalStudentFees + TotalOtherIncomes;

        public decimal TotalSalaries { get; set; }
        public decimal TotalCenterExpenses { get; set; }
        public decimal TotalOutgoings => TotalSalaries + TotalCenterExpenses;

        public decimal NetIncome => TotalIncomes - TotalOutgoings;

        public List<CenterExpenseDTO> ExpensesBreakdown { get; set; } = new List<CenterExpenseDTO>();
        public List<CenterIncomeDTO> IncomesBreakdown { get; set; } = new List<CenterIncomeDTO>();
    }
}
