using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IStudentFeeService
    {
        Task GenerateForFeePlanAsync(int feePlanId, int groupId, int month, int year);
        Task<bool> UpdatePaymentAsync(int studentFeeId, decimal amountPaid, DateOnly? paymentDate);
        Task<IEnumerable<StudentFeeViewDTO>> GetAllAsync(int groupId, int month, int year);
        Task<IEnumerable<StudentFeeViewDTO>> GetAllWithoutFilterAsync(int month, int year);
        Task<bool> ExemptStudentAsync(int studentFeeId, string reason);
        Task<bool> CancelExemptionAsync(int studentFeeId);
        Task<IEnumerable<StudentFeeViewDTO>> GetByStudentIdAsync(int studentId);
    }
}
