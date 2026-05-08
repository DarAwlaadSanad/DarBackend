using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DarV2.Service
{
    public class StudentFeeService : IStudentFeeService
    {
        private readonly IUnitOfWork _uow;

        public StudentFeeService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task GenerateForFeePlanAsync(int feePlanId,int groupId, int month, int year)
        {
            var feePlan = await _uow.FeePlans.GetByIdAsync(feePlanId);
            if (feePlan == null) throw new InvalidOperationException("FeePlan not found");

            // Get students in the group
            var students = await _uow.Students.GetStudentsGroupAsync(feePlan.GroupId);

            foreach (var student in students)
            {
                // skip if student fee already exists for that month/year
                var exists = await _uow.StudentFees.Query().AnyAsync(sf => sf.StudentId == student.Id &&sf.GroupId == groupId && sf.Month == month && sf.Year == year&&sf.RequiredAmount==feePlan.Amount);
                if (exists) continue;
                exists = await _uow.StudentFees.Query().AnyAsync(sf => sf.StudentId == student.Id && sf.GroupId == groupId && sf.Month == month && sf.Year == year);
                if (exists)
                {
                    var SF = await _uow.StudentFees.Query().FirstOrDefaultAsync(sf => sf.StudentId == student.Id && sf.GroupId == groupId && sf.Month == month && sf.Year == year);
                    if (SF != null)
                    {
                        SF.RequiredAmount = feePlan.Amount;
                        _uow.StudentFees.Update(SF);
                        await _uow.SaveAsync();
                    }
                    continue;
                }

                var sf = new StudentFee
                {
                    StudentId = student.Id,
                    RequiredAmount = feePlan.Amount,
                    GroupId= groupId,
                    Month = month,
                    Year = year,
                    AmountPaid = 0,
                    PaymentDate = null
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
            _uow.StudentFees.Update(sf);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<StudentFeeViewDTO>> GetAllAsync(int groupId, int month, int year)
        {
            return await _uow.StudentFees.Query()
                .Where(sf =>
                    sf.Month == month &&
                    sf.Year == year &&
                    sf.GroupId==groupId)
                .Select(sf => new StudentFeeViewDTO
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FullName,
                    RequiredAmount = sf.RequiredAmount,
                    AmountPaid = sf.AmountPaid,
                    Month = sf.Month,
                    Year = sf.Year,
                    PaymentDate = sf.PaymentDate
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentFeeViewDTO>> GetAllWithoutFilterAsync(int month, int year)
        {
            return await _uow.StudentFees.Query()
                .Where(sf =>
                    sf.Month == month &&
                    sf.Year == year)
                .Select(sf => new StudentFeeViewDTO
                {
                    Id = sf.Id,
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FullName,
                    RequiredAmount = sf.RequiredAmount,
                    AmountPaid = sf.AmountPaid,
                    Month = sf.Month,
                    Year = sf.Year,
                    PaymentDate = sf.PaymentDate
                })
                .ToListAsync();
        }
    }
}
