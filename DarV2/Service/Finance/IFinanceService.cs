using DarV2.DTOs.Finance;

namespace DarV2.Service.Finance
{
    public interface IFinanceService
    {
        // Settings
        Task<FinancialSettingDTO> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(FinancialSettingDTO settings);

        // Contracts
        Task<IEnumerable<UserContractDTO>> GetAllContractsAsync();
        Task<UserContractDTO?> GetContractAsync(string userId);
        Task<bool> SetContractAsync(SetUserContractDTO dto);
        Task<bool> DeleteContractAsync(string userId);

        // Transactions
        Task<IEnumerable<FinancialTransactionDTO>> GetTransactionsAsync(string userId, int month, int year);
        Task<bool> AddTransactionAsync(AddFinancialTransactionDTO dto);
        Task<bool> DeleteTransactionAsync(int id);

        // Payroll
        Task<IEnumerable<MonthlyPayrollReportDTO>> GenerateMonthlyPayrollAsync(int month, int year);
        Task<MonthlyPayrollReportDTO?> GenerateUserMonthlyPayrollAsync(string userId, int month, int year);
        Task<bool> ToggleSalaryPaymentAsync(string userId, int month, int year, decimal amount);
    }
}
