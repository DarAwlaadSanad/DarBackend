using DarV2.Service.Notification;
using DarV2.Enum;
using DarV2.Models.Finance;
using DarV2.Models;
using DarV2.Context;
using DarV2.DTOs.TeacherAttendance;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class SessionService : ISessionService
    {
        private readonly DarContext _context;
        private readonly INotificationService _notificationService;

        public SessionService(DarContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

                public async Task<bool> AssignSubstituteAsync(AssignSubstituteDTO dto)
        {
            var session = await _context.Sessions
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);
            if (session == null) return false;

            if (string.IsNullOrEmpty(dto.SubstituteTeacherId))
            {
                return await RevertSubstituteAsync(dto.SessionId);
            }

            var originalTeacherId = session.Group?.TeacherId;
            var groupName = session.Group?.Name ?? "الحلقة";

            if (dto.SubstituteTeacherId == originalTeacherId)
            {
                throw new InvalidOperationException("لا يمكن تعيين نفس المعلم الأساسي للحلقة كمعلم بديل.");
            }

            // Check if substitute teacher has any overlapping sessions on the same date
            var conflictingSessions = await _context.Sessions
                .Include(s => s.Group)
                .Where(s => s.SessionDate == session.SessionDate
                         && s.Id != session.Id
                         && (
                             // Case 1: Substitute teacher is main teacher of another group and not substituted away
                             (s.Group != null && s.Group.TeacherId == dto.SubstituteTeacherId && (string.IsNullOrEmpty(s.SubstituteTeacherId) || s.SubstituteTeacherId == dto.SubstituteTeacherId))
                             ||
                             // Case 2: Substitute teacher is already assigned as substitute for another session
                             (s.SubstituteTeacherId == dto.SubstituteTeacherId)
                         ))
                .ToListAsync();

            var conflict = conflictingSessions.FirstOrDefault(c => session.StartTime < c.EndTime && session.EndTime > c.StartTime);
            if (conflict != null)
            {
                var conflictGroup = conflict.Group?.Name ?? "حلقة أخرى";
                var startStr = conflict.StartTime.ToString(@"hh\:mm");
                var endStr = conflict.EndTime.ToString(@"hh\:mm");
                var subUser = await _context.Users.FindAsync(dto.SubstituteTeacherId);
                var teacherName = subUser?.FullName ?? subUser?.UserName ?? "المعلم";
                throw new InvalidOperationException($"المعلم ({teacherName}) غير متاح في هذا التوقيت، لديه حصة أخرى في ({conflictGroup}) من {startStr} إلى {endStr}.");
            }

            // 1. Mark original teacher as absent for this session date if not already marked
            if (!string.IsNullOrEmpty(originalTeacherId))
            {
                var attendance = await _context.TeacherAttendances
                    .FirstOrDefaultAsync(a => a.TeacherId == originalTeacherId && a.Date == session.SessionDate);

                if (attendance != null)
                {
                    attendance.IsAbsent = true;
                    if (string.IsNullOrWhiteSpace(attendance.AbsenceReason))
                    {
                        attendance.AbsenceReason = $"غياب عن حلقة {groupName}";
                    }
                }
                else
                {
                    _context.TeacherAttendances.Add(new TeacherAttendance
                    {
                        TeacherId = originalTeacherId,
                        Date = session.SessionDate,
                        IsAbsent = true,
                        AbsenceReason = $"غياب عن حلقة {groupName}"
                    });
                }
            }

            // 2. Assign substitute teacher
            session.SubstituteTeacherId = dto.SubstituteTeacherId;

            // 3. Remove any previous financial transactions for this session to prevent duplicates
            var oldTransactions = await _context.FinancialTransactions
                .Where(t => t.Reason != null && t.Reason.Contains($"(حصة #{session.Id})"))
                .ToListAsync();
            if (oldTransactions.Any())
            {
                _context.FinancialTransactions.RemoveRange(oldTransactions);
            }

            // 4. Financial addition & deduction:
            // If substitute teacher has a fixed monthly salary (FixedMonthly), add session price to their salary and deduct from absent teacher
            var subContract = await _context.UserContracts
                .FirstOrDefaultAsync(c => c.UserId == dto.SubstituteTeacherId);

            if (subContract != null && subContract.SalaryType == SalaryType.FixedMonthly)
            {
                var settings = await _context.FinancialSettings.FirstOrDefaultAsync();
                var sessionPrice = settings?.AbsenceSessionDeduction ?? 0;

                if (sessionPrice > 0)
                {
                    var txDate = session.SessionDate.ToDateTime(TimeOnly.MinValue);

                    // Add session price to substitute teacher
                    _context.FinancialTransactions.Add(new FinancialTransaction
                    {
                        UserId = dto.SubstituteTeacherId,
                        Amount = sessionPrice,
                        Type = TransactionType.SubstituteBonus,
                        Reason = $"مكافأة حصة بديلة لحلقة {groupName} (حصة #{session.Id})",
                        TransactionDate = txDate
                    });

                    // Deduct session price from absent teacher
                    if (!string.IsNullOrEmpty(originalTeacherId))
                    {
                        _context.FinancialTransactions.Add(new FinancialTransaction
                        {
                            UserId = originalTeacherId,
                            Amount = -sessionPrice,
                            Type = TransactionType.AbsenceDeduction,
                            Reason = $"خصم غياب حصة حلقة {groupName} (حصة #{session.Id})",
                            TransactionDate = txDate
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            // 5. Send Notification via NotificationService (SignalR + DB)
            try
            {
                var dateStr = session.SessionDate.ToString("yyyy/MM/dd");
                var startDateTime = DateTime.Today.Add(session.StartTime);
                var endDateTime = DateTime.Today.Add(session.EndTime);
                var startPeriod = startDateTime.Hour >= 12 ? "مساءً" : "صباحاً";
                var endPeriod = endDateTime.Hour >= 12 ? "مساءً" : "صباحاً";
                var startHour = startDateTime.Hour % 12 == 0 ? 12 : startDateTime.Hour % 12;
                var endHour = endDateTime.Hour % 12 == 0 ? 12 : endDateTime.Hour % 12;
                var timeStr = $"{startHour:D2}:{startDateTime.Minute:D2} {startPeriod} إلى {endHour:D2}:{endDateTime.Minute:D2} {endPeriod}";

                await _notificationService.CreateNotificationAsync(
                    dto.SubstituteTeacherId,
                    "تكليف كمعلم بديل",
                    $"تم تعيينك كمعلم بديل لحلقة '{groupName}' المقررة بتاريخ {dateStr} في الفترة من {timeStr}.",
                    "SubstituteAssignment",
                    session.Id,
                    session.GroupId
                );
            }
            catch
            {
            }

            return true;
        }

        public async Task<bool> RevertSubstituteAsync(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return false;

            var originalTeacherId = session.Group?.TeacherId;

            // Remove any financial transactions created for this session
            var relatedTransactions = await _context.FinancialTransactions
                .Where(t => t.Reason != null && t.Reason.Contains($"(حصة #{session.Id})"))
                .ToListAsync();
            if (relatedTransactions.Any())
            {
                _context.FinancialTransactions.RemoveRange(relatedTransactions);
            }

            // Clear substitute teacher
            session.SubstituteTeacherId = null;

            // Check if the original teacher has any other substitute sessions on that day
            if (!string.IsNullOrEmpty(originalTeacherId))
            {
                var hasOtherSubstituteSessions = await _context.Sessions
                    .AnyAsync(s => s.Id != sessionId 
                                && s.SessionDate == session.SessionDate 
                                && s.Group != null 
                                && s.Group.TeacherId == originalTeacherId 
                                && s.SubstituteTeacherId != null);

                if (!hasOtherSubstituteSessions)
                {
                    var attendance = await _context.TeacherAttendances
                        .FirstOrDefaultAsync(a => a.TeacherId == originalTeacherId && a.Date == session.SessionDate);
                    if (attendance != null && attendance.IsAbsent && attendance.AbsenceReason != null && attendance.AbsenceReason.StartsWith("غياب عن حلقة"))
                    {
                        attendance.IsAbsent = false;
                        attendance.AbsenceReason = null;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetTeacherSessionsByDateAsync(string teacherId, DateOnly date)
        {
            var sessions = await _context.Sessions
                .Include(s => s.Group)
                .Include(s => s.SubstituteTeacher)
                .Where(s => s.SessionDate == date && s.Group.TeacherId == teacherId)
                .Select(s => new
                {
                    Id = s.Id,
                    GroupId = s.GroupId,
                    GroupName = s.Group.Name,
                    SessionDate = s.SessionDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SubstituteTeacherId = s.SubstituteTeacherId,
                    SubstituteTeacherName = s.SubstituteTeacher != null ? s.SubstituteTeacher.FullName : null
                })
                .ToListAsync();

            return sessions;
        }

        public async Task<IEnumerable<object>> GetTodaySessionsAsync(string userId, bool isAdmin, bool isTeacher)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.Sessions
                .Include(s => s.Group)
                .ThenInclude(g => g.Teacher)
                .Include(s => s.SubstituteTeacher)
                .Where(s => s.SessionDate == today);

            if (!isAdmin && isTeacher)
            {
                query = query.Where(s => s.Group.TeacherId == userId || s.SubstituteTeacherId == userId);
            }
            else if (!isAdmin && !isTeacher)
            {
                return Enumerable.Empty<object>();
            }

            var sessions = await query
                .Select(s => new
                {
                    Id = s.Id,
                    GroupId = s.GroupId,
                    GroupName = s.Group.Name,
                    SessionDate = s.SessionDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    TeacherId = s.Group.TeacherId,
                    TeacherName = s.Group.Teacher != null ? s.Group.Teacher.FullName : null,
                    SubstituteTeacherId = s.SubstituteTeacherId,
                    SubstituteTeacherName = s.SubstituteTeacher != null ? s.SubstituteTeacher.FullName : null
                })
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return sessions;
        }
    }
}
