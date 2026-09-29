using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class StudentFeeService : IStudentFeeService
    {
        private readonly IUnitOfWork _uow;

        public StudentFeeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task GenerateForFeePlanAsync(int feePlanId, int groupId, int month, int year)
        {
            var feePlan = await _uow.FeePlans.GetByIdAsync(feePlanId);
            if (feePlan == null) throw new InvalidOperationException("FeePlan not found");

            var students = await _uow.Students.GetStudentsGroupAsync(feePlan.GroupId);

            foreach (var student in students)
            {
                var exists = await _uow.StudentFees.Query()
                    .AnyAsync(sf => sf.StudentId == student.Id && sf.GroupId == groupId
                                 && sf.Month == month && sf.Year == year && sf.RequiredAmount == feePlan.Amount);
                if (exists) continue;

                var existing = await _uow.StudentFees.Query()
                    .FirstOrDefaultAsync(sf => sf.StudentId == student.Id && sf.GroupId == groupId
                                            && sf.Month == month && sf.Year == year);

                if (existing != null)
                {
                    existing.RequiredAmount = feePlan.Amount;
                    // Apply permanent exemption if set on student
                    if (student.IsFeeExempted && !existing.IsExempted)
                    {
                        existing.IsExempted = true;
                        existing.ExemptionReason = student.FeeExemptionReason ?? "إعفاء دائم";
                    }
                    _uow.StudentFees.Update(existing);
                    await _uow.SaveAsync();
                    continue;
                }

                var sf = new StudentFee
                {
                    StudentId = student.Id,
                    RequiredAmount = feePlan.Amount,
                    GroupId = groupId,
                    Month = month,
                    Year = year,
                    AmountPaid = 0,
                    PaymentDate = null,
                    // Auto-exempt if student has a permanent exemption
                    IsExempted = student.IsFeeExempted,
                    ExemptionReason = student.IsFeeExempted ? (student.FeeExemptionReason ?? "إعفاء دائم") : null
                };

                await _uow.StudentFees.AddAsync(sf);
            }

            await _uow.SaveAsync();
        }

        public async Task<bool> UpdatePaymentAsync(int studentFeeId, decimal amountPaid, DateOnly? paymentDate)
        {
            var sf = await _uow.StudentFees.GetByIdAsync(studentFeeId);
            if (sf == null) return false;

            sf.AmountPaid = amountPaid;
            sf.PaymentDate = paymentDate;
            // If a payment is made on an exempted fee, keep IsExempted but record the payment
            _uow.StudentFees.Update(sf);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> ExemptStudentAsync(int studentFeeId, string reason)
        {
            var sf = await _uow.StudentFees
                .Query()
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == studentFeeId);
            if (sf == null) return false;

            // Mark the current fee as exempted
            sf.IsExempted = true;
            sf.ExemptionReason = reason;

            // Also mark the student as permanently exempted so future fees are auto-exempted
            if (sf.Student != null)
            {
                sf.Student.IsFeeExempted = true;
                sf.Student.FeeExemptionReason = reason;
            }

            _uow.StudentFees.Update(sf);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> CancelExemptionAsync(int studentFeeId)
        {
            var sf = await _uow.StudentFees
                .Query()
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == studentFeeId);
            if (sf == null) return false;

            // Cancel exemption on this fee record
            sf.IsExempted = false;
            sf.ExemptionReason = null;

            // Also remove permanent exemption from the student
            if (sf.Student != null)
            {
                sf.Student.IsFeeExempted = false;
                sf.Student.FeeExemptionReason = null;
            }

            _uow.StudentFees.Update(sf);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<StudentFeeViewDTO>> GetAllAsync(int groupId, int month, int year)
        {
            return await _uow.StudentFees.Query()
                .Where(sf => sf.Month == month && sf.Year == year && sf.GroupId == groupId)
                .Select(sf => new StudentFeeViewDTO
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FullName,
                    Gender = sf.Student.Gender,
                    GroupId = sf.GroupId,
                    GroupName = sf.Group.Name,
                    RequiredAmount = sf.RequiredAmount,
                    AmountPaid = sf.AmountPaid,
                    Month = sf.Month,
                    Year = sf.Year,
                    PaymentDate = sf.PaymentDate,
                    IsExempted = sf.IsExempted,
                    ExemptionReason = sf.ExemptionReason,
                    IsPermanentlyExempted = sf.Student.IsFeeExempted
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFeeViewDTO>> GetAllWithoutFilterAsync(int month, int year)
        {
            return await _uow.StudentFees.Query()
                .Where(sf => sf.Month == month && sf.Year == year)
                .Select(sf => new StudentFeeViewDTO
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FullName,
                    Gender = sf.Student.Gender,
                    GroupId = sf.GroupId,
                    GroupName = sf.Group.Name,
                    RequiredAmount = sf.RequiredAmount,
                    AmountPaid = sf.AmountPaid,
                    Month = sf.Month,
                    Year = sf.Year,
                    PaymentDate = sf.PaymentDate,
                    IsExempted = sf.IsExempted,
                    ExemptionReason = sf.ExemptionReason,
                    IsPermanentlyExempted = sf.Student.IsFeeExempted
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFeeViewDTO>> GetByStudentIdAsync(int studentId)
        {
            return await _uow.StudentFees.Query()
                .Where(sf => sf.StudentId == studentId)
                .OrderByDescending(sf => sf.Year)
                .ThenByDescending(sf => sf.Month)
                .Select(sf => new StudentFeeViewDTO
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FullName,
                    Gender = sf.Student.Gender,
                    GroupId = sf.GroupId,
                    GroupName = sf.Group.Name,
                    RequiredAmount = sf.RequiredAmount,
                    AmountPaid = sf.AmountPaid,
                    Month = sf.Month,
                    Year = sf.Year,
                    PaymentDate = sf.PaymentDate,
                    IsExempted = sf.IsExempted,
                    ExemptionReason = sf.ExemptionReason,
                    IsPermanentlyExempted = sf.Student.IsFeeExempted
                })
                .ToListAsync();
        }
    }
}