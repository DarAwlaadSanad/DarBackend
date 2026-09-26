using System.Collections.Generic;
using System.Threading.Tasks;
using DarV2.DTOs.Finance;

namespace DarV2.Service.Finance
{
    public interface ICenterFinanceService
    {
        // Expenses
        Task<List<CenterExpenseDTO>> GetExpensesAsync(int month, int year);
        Task<CenterExpenseDTO> AddExpenseAsync(CreateCenterExpenseDTO dto);
        Task DeleteExpenseAsync(int id);

        // Incomes (Donations, etc.)
        Task<List<CenterIncomeDTO>> GetIncomesAsync(int month, int year);
        Task<CenterIncomeDTO> AddIncomeAsync(CreateCenterIncomeDTO dto);
        Task DeleteIncomeAsync(int id);

        // Summary
        Task<MonthlyFinancialSummaryDTO> GetMonthlySummaryAsync(int month, int year);
    }
}
