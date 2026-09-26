using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class StudentWarningService : IStudentWarningService
    {
        private readonly IUnitOfWork _uow;

        public StudentWarningService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<StudentWarningViewDTO> CreateAsync(StudentWarningCreateDTO dto, string? createdByUserId)
        {
            var student = await _uow.Students.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                throw new InvalidOperationException("الطالب غير موجود");
            }

            var warning = new StudentWarning
            {
                StudentId = dto.StudentId,
                WarningType = dto.WarningType,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                Reason = dto.Reason,
                GroupId = dto.GroupId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.StudentWarnings.AddAsync(warning);
            await _uow.SaveAsync();

            var loaded = await _uow.StudentWarnings.Query()
                .Include(w => w.Student)
                .Include(w => w.Group)
                .Include(w => w.CreatedByUser)
                .FirstOrDefaultAsync(w => w.Id == warning.Id);

            return MapToDTO(loaded ?? warning);
        }

        public async Task<StudentWarningViewDTO?> GetByIdAsync(int id)
        {
            var warning = await _uow.StudentWarnings.Query()
                .Include(w => w.Student)
                .Include(w => w.Group)
                .Include(w => w.CreatedByUser)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warning == null) return null;
            return MapToDTO(warning);
        }

        public async Task<IEnumerable<StudentWarningViewDTO>> GetByStudentIdAsync(int studentId)
        {
            var list = await _uow.StudentWarnings.Query()
                .Include(w => w.Student)
                .Include(w => w.Group)
                .Include(w => w.CreatedByUser)
                .Where(w => w.StudentId == studentId)
                .OrderByDescending(w => w.Date)
                .ThenByDescending(w => w.CreatedAt)
                .ToListAsync();

            return list.Select(MapToDTO).ToList();
        }

        public async Task<StudentWarningPagedResultDTO> GetAllAsync(int page, int pageSize, int? studentId, WarningType? warningType, int? groupId, DateTime? fromDate, DateTime? toDate, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _uow.StudentWarnings.Query()
                .Include(w => w.Student)
                .Include(w => w.Group)
                .Include(w => w.CreatedByUser)
                .AsQueryable();

            if (studentId.HasValue && studentId.Value > 0)
            {
                query = query.Where(w => w.StudentId == studentId.Value);
            }

            if (warningType.HasValue)
            {
                query = query.Where(w => w.WarningType == warningType.Value);
            }

            if (groupId.HasValue && groupId.Value > 0)
            {
                query = query.Where(w => w.GroupId == groupId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(w => w.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(w => w.Date <= endOfDay);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(w => w.Student.FullName.Contains(s) ||
                                         w.Student.Code.Contains(s) ||
                                         (w.Reason != null && w.Reason.Contains(s)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(w => w.Date)
                .ThenByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new StudentWarningPagedResultDTO
            {
                Items = items.Select(MapToDTO).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<StudentWarningSummaryDTO> GetSummaryAsync(int? studentId)
        {
            var query = _uow.StudentWarnings.Query().AsQueryable();
            if (studentId.HasValue && studentId.Value > 0)
            {
                query = query.Where(w => w.StudentId == studentId.Value);
            }

            var total = await query.CountAsync();
            var absence = await query.CountAsync(w => w.WarningType == WarningType.Absence);
            var misbehavior = await query.CountAsync(w => w.WarningType == WarningType.Misbehavior);
            var notMemorized = await query.CountAsync(w => w.WarningType == WarningType.NotMemorized);
            var other = await query.CountAsync(w => w.WarningType == WarningType.Other);

            return new StudentWarningSummaryDTO
            {
                TotalCount = total,
                AbsenceCount = absence,
                MisbehaviorCount = misbehavior,
                NotMemorizedCount = notMemorized,
                OtherCount = other
            };
        }

        public async Task<bool> UpdateAsync(int id, StudentWarningUpdateDTO dto)
        {
            var warning = await _uow.StudentWarnings.GetByIdAsync(id);
            if (warning == null) return false;

            warning.WarningType = dto.WarningType;
            warning.Date = dto.Date == default ? warning.Date : dto.Date;
            warning.Reason = dto.Reason;
            warning.GroupId = dto.GroupId;

            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warning = await _uow.StudentWarnings.GetByIdAsync(id);
            if (warning == null) return false;

            _uow.StudentWarnings.Remove(warning);
            await _uow.SaveAsync();
            return true;
        }

        private static StudentWarningViewDTO MapToDTO(StudentWarning w)
        {
            return new StudentWarningViewDTO
            {
                Id = w.Id,
                StudentId = w.StudentId,
                StudentName = w.Student?.FullName ?? string.Empty,
                StudentCode = w.Student?.Code ?? string.Empty,
                WarningType = w.WarningType,
                WarningTypeName = GetWarningTypeName(w.WarningType),
                Date = w.Date,
                Reason = w.Reason,
                GroupId = w.GroupId,
                GroupName = w.Group?.Name,
                CreatedByUserId = w.CreatedByUserId,
                CreatedByName = w.CreatedByUser?.UserName ?? w.CreatedByUser?.PhoneNumber,
                CreatedAt = w.CreatedAt
            };
        }

        public static string GetWarningTypeName(WarningType type)
        {
            return type switch
            {
                WarningType.Absence => "إنذار غياب",
                WarningType.Misbehavior => "إنذار شغب",
                WarningType.NotMemorized => "إنذار عدم حفظ",
                WarningType.Other => "إنذار آخر",
                _ => "إنذار"
            };
        }
    }
}
