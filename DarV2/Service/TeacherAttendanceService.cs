using DarV2.Context;
using DarV2.DTOs.TeacherAttendance;
using DarV2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarV2.Service
{
    public class TeacherAttendanceService : ITeacherAttendanceService
    {
        private readonly DarContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherAttendanceService(
            DarContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<TeacherAttendanceRecordDTO?> GetTodayRecordAsync(string teacherId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date == today);

            if (record == null) return null;

            return MapToDTO(record);
        }

        private async Task<bool> DoesUserBypassSessionRequirementAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                               r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                               r.Equals("مشرف", StringComparison.OrdinalIgnoreCase) ||
                               r.Equals("Supervisor", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check user claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            if (userClaims.Any(c => c.Type == "Permission" && c.Value == Permissions.BypassAttendanceSessionRequirement))
            {
                return true;
            }

            // Check role claims
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    if (roleClaims.Any(c => c.Type == "Permission" && c.Value == Permissions.BypassAttendanceSessionRequirement))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<(bool HasSessions, TimeSpan? EarliestStart, TimeSpan? LatestEnd, int Count)> GetTodaySessionsInfoAsync(string teacherId, DateOnly today)
        {
            var currentDayOfWeek = (DayOfWeekAr)(int)DateTime.Today.DayOfWeek;

            // 1. Check generated Sessions for today
            var sessionsToday = await _context.Sessions
                .Include(s => s.Group)
                .Where(s => s.SessionDate == today && (
                    s.SubstituteTeacherId == teacherId ||
                    (s.Group.TeacherId == teacherId && (string.IsNullOrEmpty(s.SubstituteTeacherId) || s.SubstituteTeacherId == teacherId))
                ))
                .ToListAsync();

            if (sessionsToday.Any())
            {
                var earliest = sessionsToday.Min(s => s.StartTime);
                var latest = sessionsToday.Max(s => s.EndTime);
                return (true, earliest, latest, sessionsToday.Count);
            }

            // 2. Fallback: check GroupSchedules if no explicit Session entities exist
            var schedules = await _context.Groups
                .Where(g => g.TeacherId == teacherId)
                .SelectMany(g => g.Schedules)
                .Where(s => s.DayOfWeek == currentDayOfWeek && s.IsActive)
                .ToListAsync();

            if (schedules.Any())
            {
                var earliest = schedules.Min(s => s.StartTime);
                var latest = schedules.Max(s => s.EndTime);
                return (true, earliest, latest, schedules.Count);
            }

            return (false, null, null, 0);
        }

        public async Task<TodayAttendanceStatusDTO> GetTodayAttendanceStatusAsync(string teacherId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var record = await GetTodayRecordAsync(teacherId);

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);
            var (hasSessions, earliestStart, latestEnd, count) = await GetTodaySessionsInfoAsync(teacherId, today);

            bool requiresSessions = !bypass;
            bool canCheckIn = bypass || hasSessions;

            string? message = null;
            if (requiresSessions && !hasSessions)
            {
                message = "ليس لديك حصص لليوم (كمعلم أساسي أو كمعلم بديل).";
            }

            return new TodayAttendanceStatusDTO
            {
                HasSessionsToday = hasSessions,
                RequiresSessions = requiresSessions,
                CanCheckIn = canCheckIn,
                Message = message,
                Record = record,
                SessionsCount = count
            };
        }

        public async Task<CheckInResponseDTO> CheckInAsync(string teacherId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var currentTime = DateTime.Now.TimeOfDay;

            // Check if already checked in
            var existingRecord = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date == today);

            if (existingRecord != null)
            {
                return new CheckInResponseDTO
                {
                    Success = false,
                    Message = "تم تسجيل الحضور مسبقاً لهذا اليوم.",
                    DelayMinutes = existingRecord.DelayMinutes,
                    Record = MapToDTO(existingRecord)
                };
            }

            var user = await _userManager.FindByIdAsync(teacherId);
            if (user == null) return new CheckInResponseDTO { Success = false, Message = "المستخدم غير موجود." };

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);
            int delayMinutes = 0;

            if (!bypass)
            {
                var (hasSessions, earliestSessionStart, _, _) = await GetTodaySessionsInfoAsync(teacherId, today);

                if (!hasSessions || earliestSessionStart == null)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "لا يمكنك تسجيل الحضور لأنه ليس لديك حصص لليوم (كمعلم أساسي أو كمعلم بديل)."
                    };
                }

                var allowedCheckInTime = earliestSessionStart.Value.Subtract(TimeSpan.FromMinutes(30));

                if (currentTime < allowedCheckInTime)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = $"لا يمكن تسجيل الحضور الآن. مسموح بتسجيل الحضور قبل أول حصة بـ 30 دقيقة كحد أقصى. (يبدأ الحضور من {allowedCheckInTime:hh\\:mm})"
                    };
                }

                var requiredCheckInTime = earliestSessionStart.Value.Subtract(TimeSpan.FromMinutes(5));
                var delay = currentTime - requiredCheckInTime;
                delayMinutes = delay.TotalMinutes > 0 ? (int)delay.TotalMinutes : 0;
            }

            var newRecord = new TeacherAttendance
            {
                TeacherId = teacherId,
                Date = today,
                CheckInTime = currentTime,
                DelayMinutes = delayMinutes
            };

            _context.TeacherAttendances.Add(newRecord);
            await _context.SaveChangesAsync();

            return new CheckInResponseDTO
            {
                Success = true,
                Message = "تم تسجيل الحضور بنجاح.",
                DelayMinutes = delayMinutes,
                Record = MapToDTO(newRecord)
            };
        }

        public async Task<CheckOutResponseDTO> CheckOutAsync(string teacherId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var currentTime = DateTime.Now.TimeOfDay;

            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date == today);

            if (record == null)
            {
                return new CheckOutResponseDTO
                {
                    Success = false,
                    Message = "لم يتم العثور على سجل حضور لهذا اليوم. يجب تسجيل الحضور أولاً.",
                };
            }

            if (record.CheckOutTime.HasValue)
            {
                return new CheckOutResponseDTO
                {
                    Success = false,
                    Message = "تم تسجيل الانصراف مسبقاً لهذا اليوم.",
                    Record = MapToDTO(record)
                };
            }

            var user = await _userManager.FindByIdAsync(teacherId);
            if (user == null) return new CheckOutResponseDTO { Success = false, Message = "المستخدم غير موجود." };

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);

            if (!bypass)
            {
                var (hasSessions, _, latestSessionEnd, _) = await GetTodaySessionsInfoAsync(teacherId, today);

                if (latestSessionEnd != null)
                {
                    var allowedCheckOutTime = latestSessionEnd.Value.Add(TimeSpan.FromMinutes(5));

                    if (currentTime < allowedCheckOutTime)
                    {
                        return new CheckOutResponseDTO
                        {
                            Success = false,
                            Message = $"لا يمكن تسجيل الانصراف الآن. يجب الانتظار 5 دقائق بعد انتهاء آخر حصة. (يمكنك الانصراف بداية من {allowedCheckOutTime:hh\\:mm})"
                        };
                    }
                }
            }

            record.CheckOutTime = currentTime;
            await _context.SaveChangesAsync();

            return new CheckOutResponseDTO
            {
                Success = true,
                Message = "تم تسجيل الانصراف بنجاح.",
                Record = MapToDTO(record)
            };
        }

        public async Task<bool> MarkTeacherAbsentAsync(MarkTeacherAbsentDTO dto)
        {
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == dto.TeacherId && a.Date == dto.Date);

            if (record != null)
            {
                record.IsAbsent = true;
                record.AbsenceReason = dto.Reason;
                record.CheckInTime = null;
                record.CheckOutTime = null;
                record.DelayMinutes = 0;
            }
            else
            {
                record = new TeacherAttendance
                {
                    TeacherId = dto.TeacherId,
                    Date = dto.Date,
                    IsAbsent = true,
                    AbsenceReason = dto.Reason
                };
                _context.TeacherAttendances.Add(record);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<bool> CancelTeacherAbsentAsync(MarkTeacherAbsentDTO dto)
        {
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == dto.TeacherId && a.Date == dto.Date);

            if (record != null)
            {
                record.IsAbsent = false;
                record.AbsenceReason = null;
                await _context.SaveChangesAsync();
            }

            return true;
        }

public async Task<List<TeacherMonthlyAttendanceReportDTO>> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendances = await _context.TeacherAttendances
                .Include(a => a.Teacher)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            var allUsers = await _userManager.Users.ToListAsync();

            var report = new List<TeacherMonthlyAttendanceReportDTO>();

            foreach (var teacher in allUsers)
            {
                var teacherRecords = attendances.Where(a => a.TeacherId == teacher.Id).ToList();

                var dto = new TeacherMonthlyAttendanceReportDTO
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.FullName,
                    AbsentDays = teacherRecords.Count(r => r.IsAbsent),
                    TotalLateMinutes = teacherRecords.Sum(r => r.DelayMinutes),
                    AbsentSessions = 0,
                    LateSessions = 0
                };

                report.Add(dto);
            }

            return report;
        }

        private TeacherAttendanceRecordDTO MapToDTO(TeacherAttendance record)
        {
            return new TeacherAttendanceRecordDTO
            {
                Id = record.Id,
                TeacherId = record.TeacherId,
                Date = record.Date.ToDateTime(TimeOnly.MinValue),
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                DelayMinutes = record.DelayMinutes,
                IsAbsent = record.IsAbsent,
                AbsenceReason = record.AbsenceReason
            };
        }
    }
}