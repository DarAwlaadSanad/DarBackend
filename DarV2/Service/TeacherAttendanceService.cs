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

        /// <summary>
        /// Gets the current date and time in Egypt, strictly adhering to Egypt Daylight Saving Time (UTC+3 in summer, UTC+2 in winter)
        /// regardless of the host server operating system or cloud timezone settings.
        /// </summary>
        public static DateTime GetEgyptNow()
        {
            var utc = DateTime.UtcNow;
            int year = utc.Year;

            // Egyptian Daylight Saving Time (Law No. 24 of 2023):
            // Starts last Friday of April, ends last Thursday of October
            var lastFridayApril = new DateTime(year, 4, 30);
            while (lastFridayApril.DayOfWeek != DayOfWeek.Friday)
                lastFridayApril = lastFridayApril.AddDays(-1);

            var lastThursdayOct = new DateTime(year, 10, 31);
            while (lastThursdayOct.DayOfWeek != DayOfWeek.Thursday)
                lastThursdayOct = lastThursdayOct.AddDays(-1);

            bool isSummer = utc >= lastFridayApril && utc < lastThursdayOct.AddDays(1);
            return isSummer ? utc.AddHours(3) : utc.AddHours(2);
        }

        public async Task<TeacherAttendanceRecordDTO?> GetTodayRecordAsync(string teacherId)
        {
            var today = DateOnly.FromDateTime(GetEgyptNow());
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date == today);

            if (record == null) return null;

            // Ghost record safeguard: if check-in is null and not absent, treat as null
            if (record.CheckInTime == null && !record.IsAbsent)
            {
                return null;
            }

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

        private async Task<List<(TimeSpan StartTime, TimeSpan EndTime)>> GetTodaySessionsListAsync(string teacherId, DateOnly today)
        {
            var currentDayOfWeek = (DayOfWeekAr)(int)GetEgyptNow().DayOfWeek;

            // 1. Check generated Sessions for today
            var sessionsToday = await _context.Sessions
                .Include(s => s.Group)
                .Where(s => s.SessionDate == today && (
                    s.SubstituteTeacherId == teacherId ||
                    (s.Group.TeacherId == teacherId && (string.IsNullOrEmpty(s.SubstituteTeacherId) || s.SubstituteTeacherId == teacherId))
                ))
                .OrderBy(s => s.StartTime)
                .Select(s => new { s.StartTime, s.EndTime })
                .ToListAsync();

            if (sessionsToday.Any())
            {
                return sessionsToday.Select(s => (s.StartTime, s.EndTime)).ToList();
            }

            // 2. Fallback: check GroupSchedules if no explicit Session entities exist
            var schedules = await _context.Groups
                .Where(g => g.TeacherId == teacherId)
                .SelectMany(g => g.Schedules)
                .Where(s => s.DayOfWeek == currentDayOfWeek && s.IsActive)
                .OrderBy(s => s.StartTime)
                .Select(s => new { s.StartTime, s.EndTime })
                .ToListAsync();

            return schedules.Select(s => (s.StartTime, s.EndTime)).ToList();
        }

        private List<TeacherPeriodDTO> GroupSessionsIntoPeriods(List<(TimeSpan StartTime, TimeSpan EndTime)> sessions)
        {
            var periods = new List<TeacherPeriodDTO>();
            if (!sessions.Any()) return periods;

            var ordered = sessions.OrderBy(s => s.StartTime).ToList();

            int periodNum = 1;
            var currentStart = ordered[0].StartTime;
            var currentEnd = ordered[0].EndTime;
            int count = 1;

            for (int i = 1; i < ordered.Count; i++)
            {
                var nextSession = ordered[i];
                // Consecutive sessions: break between them <= 30 minutes
                if (nextSession.StartTime <= currentEnd.Add(TimeSpan.FromMinutes(30)))
                {
                    if (nextSession.EndTime > currentEnd)
                    {
                        currentEnd = nextSession.EndTime;
                    }
                    count++;
                }
                else
                {
                    // Separated sessions: save current period and start a new period
                    periods.Add(new TeacherPeriodDTO
                    {
                        PeriodNumber = periodNum++,
                        StartTime = currentStart,
                        EndTime = currentEnd,
                        SessionsCount = count
                    });

                    currentStart = nextSession.StartTime;
                    currentEnd = nextSession.EndTime;
                    count = 1;
                }
            }

            periods.Add(new TeacherPeriodDTO
            {
                PeriodNumber = periodNum,
                StartTime = currentStart,
                EndTime = currentEnd,
                SessionsCount = count
            });

            return periods;
        }

        public async Task<TodayAttendanceStatusDTO> GetTodayAttendanceStatusAsync(string teacherId)
        {
            var egyptNow = GetEgyptNow();
            var today = DateOnly.FromDateTime(egyptNow);
            var currentTime = egyptNow.TimeOfDay;

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);
            var sessionTimes = await GetTodaySessionsListAsync(teacherId, today);
            var periods = GroupSessionsIntoPeriods(sessionTimes);

            bool hasSessions = periods.Any();
            bool requiresSessions = hasSessions || !bypass;
            bool canCheckIn = bypass || hasSessions;

            string? message = null;
            if (requiresSessions && !hasSessions)
            {
                message = "ليس لديك حصص لليوم (كمعلم أساسي أو كمعلم بديل).";
            }

            var allTodayRecords = await _context.TeacherAttendances
                .Where(a => a.TeacherId == teacherId && a.Date == today)
                .OrderBy(a => a.Id)
                .ToListAsync();

            // Filter out ghost records (check-in null and not absent)
            var validRecords = allTodayRecords
                .Where(a => a.CheckInTime.HasValue || a.IsAbsent)
                .ToList();

            // Match records with periods
            TeacherAttendance? activeRecord = null;
            int currentPeriodNumber = 1;

            if (periods.Any())
            {
                bool dbModified = false;
                for (int i = 0; i < periods.Count; i++)
                {
                    var p = periods[i];
                    var rec = (i < validRecords.Count) ? validRecords[i] : null;
                    var departureTime = p.EndTime.Subtract(TimeSpan.FromMinutes(10));
                    if (departureTime <= p.StartTime) departureTime = p.EndTime;

                    if (rec != null)
                    {
                        if (rec.IsAbsent)
                        {
                            p.Status = "Absent";
                        }
                        else if (rec.CheckOutTime.HasValue)
                        {
                            p.Status = "Completed";
                        }
                        else if (!rec.CheckInTime.HasValue)
                        {
                            if (currentTime >= departureTime)
                            {
                                rec.IsAbsent = true;
                                rec.AbsenceReason = periods.Count > 1
                                    ? $"غياب لعدم تسجيل الحضور قبل موعد الانصراف (فترة {p.PeriodNumber}: {p.StartTime:hh\\:mm} - {p.EndTime:hh\\:mm})"
                                    : "غياب لعدم تسجيل الحضور قبل موعد الانصراف";
                                dbModified = true;
                                p.Status = "Absent";
                            }
                        }
                        else
                        {
                            p.Status = "Active";
                            activeRecord = rec;
                            currentPeriodNumber = p.PeriodNumber;
                        }
                    }
                    else
                    {
                        // No record for this period yet
                        if (currentTime >= departureTime)
                        {
                            // Departure time has arrived and teacher has not checked in!
                            rec = new TeacherAttendance
                            {
                                TeacherId = teacherId,
                                Date = today,
                                IsAbsent = true,
                                AbsenceReason = periods.Count > 1
                                    ? $"غياب لعدم تسجيل الحضور قبل موعد الانصراف (فترة {p.PeriodNumber}: {p.StartTime:hh\\:mm} - {p.EndTime:hh\\:mm})"
                                    : "غياب لعدم تسجيل الحضور قبل موعد الانصراف"
                            };
                            _context.TeacherAttendances.Add(rec);
                            validRecords.Add(rec);
                            dbModified = true;
                            p.Status = "Absent";
                        }
                        else if (currentTime > p.EndTime)
                        {
                            p.Status = "Passed";
                        }
                        else
                        {
                            p.Status = "Upcoming";
                        }
                    }
                }

                if (dbModified)
                {
                    await _context.SaveChangesAsync();
                }

                // If no active open record, find next upcoming period
                if (activeRecord == null)
                {
                    var nextPeriod = periods.FirstOrDefault(p => p.Status == "Upcoming") ?? periods.Last();
                    currentPeriodNumber = nextPeriod.PeriodNumber;

                    if (nextPeriod.Status == "Upcoming")
                    {
                        // Target period is upcoming and has not been attended yet
                        activeRecord = null;
                    }
                    else
                    {
                        int idx = nextPeriod.PeriodNumber - 1;
                        if (idx >= 0 && idx < validRecords.Count)
                        {
                            activeRecord = validRecords[idx];
                        }
                        else
                        {
                            activeRecord = validRecords.LastOrDefault();
                        }
                    }
                }
            }
            else
            {
                activeRecord = validRecords.LastOrDefault();
            }

            bool isCheckInOpen = true;
            string? allowedCheckInTimeStr = null;
            int? secondsUntilCheckIn = null;

            if (periods.Any())
            {
                // Find next upcoming period that has not reached departure time
                var targetPeriod = periods.FirstOrDefault(p => p.Status == "Upcoming" && currentTime < (p.EndTime.Subtract(TimeSpan.FromMinutes(10)) <= p.StartTime ? p.EndTime : p.EndTime.Subtract(TimeSpan.FromMinutes(10))));
                if (targetPeriod != null)
                {
                    var allowedTime = targetPeriod.StartTime.Subtract(TimeSpan.FromMinutes(5));
                    allowedCheckInTimeStr = allowedTime.ToString(@"hh\:mm\:ss");

                    if (currentTime < allowedTime)
                    {
                        isCheckInOpen = false;
                        var diff = allowedTime - currentTime;
                        secondsUntilCheckIn = (int)diff.TotalSeconds;
                    }
                }
                else
                {
                    // No upcoming periods eligible for check-in
                    isCheckInOpen = false;
                    canCheckIn = false;
                }
            }

            return new TodayAttendanceStatusDTO
            {
                HasSessionsToday = hasSessions,
                RequiresSessions = requiresSessions,
                CanCheckIn = canCheckIn,
                IsCheckInOpen = isCheckInOpen,
                AllowedCheckInTime = allowedCheckInTimeStr,
                SecondsUntilCheckIn = secondsUntilCheckIn,
                Message = message,
                Record = activeRecord != null ? MapToDTO(activeRecord) : null,
                SessionsCount = sessionTimes.Count,
                Periods = periods,
                AllTodayRecords = validRecords.Select(MapToDTO).ToList(),
                CurrentPeriodNumber = currentPeriodNumber
            };
        }

        public async Task<CheckInResponseDTO> CheckInAsync(string teacherId, double? latitude = null, double? longitude = null)
        {
            var egyptNow = GetEgyptNow();
            var today = DateOnly.FromDateTime(egyptNow);
            var currentTime = egyptNow.TimeOfDay;

            // --- GEOFENCING VALIDATION ---
            var activeLocations = await _context.AttendanceLocations
                .Where(a => a.IsActive)
                .ToListAsync();

            if (activeLocations.Any())
            {
                if (!latitude.HasValue || !longitude.HasValue)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "يجب تفعيل وإتاحة مشاركة الموقع الجغرافي (GPS) في المتصفح للتحقق من تواجدك في مقر المركز لتسجيل الحضور."
                    };
                }

                double minDistance = double.MaxValue;
                Models.AttendanceLocation.AttendanceLocation? closestLocation = null;

                foreach (var loc in activeLocations)
                {
                    var dist = CalculateDistanceInMeters(latitude.Value, longitude.Value, loc.Latitude, loc.Longitude);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestLocation = loc;
                    }
                }

                var allowedRadius = closestLocation?.RadiusInMeters ?? 50.0;
                if (minDistance > allowedRadius)
                {
                    var distInt = (int)Math.Round(minDistance);
                    var radInt = (int)Math.Round(allowedRadius);
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = $"عذراً، لا يمكنك تسجيل الحضور لأنك لست داخل النطاق الجغرافي للمركز ({closestLocation?.Name}). المسافة الحالية: {distInt} متر، والحد المسموح به: {radInt} متر."
                    };
                }
            }
            // --- END GEOFENCING VALIDATION ---

            var allTodayRecords = await _context.TeacherAttendances
                .Where(a => a.TeacherId == teacherId && a.Date == today)
                .OrderBy(a => a.Id)
                .ToListAsync();

            // Check if there is already an open check-in without check-out
            var openRecord = allTodayRecords.FirstOrDefault(a => a.CheckInTime.HasValue && !a.CheckOutTime.HasValue);
            if (openRecord != null)
            {
                return new CheckInResponseDTO
                {
                    Success = false,
                    Message = "لديك تسجيل حضور مفتوح حالياً. يجب تسجيل الانصراف أولاً قبل تسجيل حضور جديد.",
                    DelayMinutes = openRecord.DelayMinutes,
                    Record = MapToDTO(openRecord)
                };
            }

            var user = await _userManager.FindByIdAsync(teacherId);
            if (user == null) return new CheckInResponseDTO { Success = false, Message = "المستخدم غير موجود." };

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);
            int delayMinutes = 0;

            var sessionTimes = await GetTodaySessionsListAsync(teacherId, today);
            var periods = GroupSessionsIntoPeriods(sessionTimes);

            if (periods.Any())
            {

                // Count processed periods today (attended or marked absent) to know which period we are checking in for
                var processedPeriodsCount = allTodayRecords.Count(a => a.CheckInTime.HasValue || a.IsAbsent);

                if (processedPeriodsCount >= periods.Count)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "تم الانتهاء من جميع فترات وحصص اليوم مسبقاً."
                    };
                }

                var targetPeriod = periods[processedPeriodsCount];
                var departureTime = targetPeriod.EndTime.Subtract(TimeSpan.FromMinutes(10));
                if (departureTime <= targetPeriod.StartTime) departureTime = targetPeriod.EndTime;

                if (currentTime >= departureTime)
                {
                    var rec = allTodayRecords.FirstOrDefault(a => a.Date == today && !a.CheckInTime.HasValue && !a.IsAbsent) ?? new TeacherAttendance
                    {
                        TeacherId = teacherId,
                        Date = today
                    };
                    rec.IsAbsent = true;
                    rec.AbsenceReason = periods.Count > 1
                        ? $"غياب لعدم تسجيل الحضور قبل موعد الانصراف (فترة {targetPeriod.PeriodNumber}: {targetPeriod.StartTime:hh\\:mm} - {targetPeriod.EndTime:hh\\:mm})"
                        : "غياب لعدم تسجيل الحضور قبل موعد الانصراف";
                    if (rec.Id == 0) _context.TeacherAttendances.Add(rec);
                    await _context.SaveChangesAsync();

                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "انتهى موعد تسجيل الحضور لهذه الحصة وحان موعد الانصراف، وتم تسجيلك غائباً.",
                        Record = MapToDTO(rec)
                    };
                }

                var allowedCheckInTime = targetPeriod.StartTime.Subtract(TimeSpan.FromMinutes(5));
                var allowedCheckInTimeWithGrace = allowedCheckInTime.Subtract(TimeSpan.FromSeconds(45));

                if (currentTime < allowedCheckInTimeWithGrace)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = $"لا يمكن تسجيل الحضور الآن. مسموح بتسجيل الحضور قبل بداية الفترة {targetPeriod.PeriodNumber} بـ 5 دقائق فقط. (يبدأ الحضور من {allowedCheckInTime:hh\\:mm})"
                    };
                }

                var delay = currentTime - targetPeriod.StartTime;
                delayMinutes = delay.TotalMinutes > 0 ? (int)delay.TotalMinutes : 0;
            }
            else
            {
                if (!bypass)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "لا يمكنك تسجيل الحضور لأنه ليس لديك حصص لليوم (كمعلم أساسي أو كمعلم بديل)."
                    };
                }

                // For admin/supervisor bypass users, check if marked absent
                var absentRecord = allTodayRecords.FirstOrDefault(a => a.IsAbsent);
                if (absentRecord != null)
                {
                    return new CheckInResponseDTO
                    {
                        Success = false,
                        Message = "تم تسجيلك غائباً لهذا اليوم من قِبل الإدارة. يرجى مراجعة الإدارة أولاً.",
                        Record = MapToDTO(absentRecord)
                    };
                }
            }

            // Reuse a ghost record if one exists, otherwise add new
            var ghostRecord = allTodayRecords.FirstOrDefault(a => !a.CheckInTime.HasValue && !a.IsAbsent);
            var recordToSave = ghostRecord ?? new TeacherAttendance
            {
                TeacherId = teacherId,
                Date = today
            };
            recordToSave.CheckInTime = currentTime;
            recordToSave.CheckOutTime = null;
            recordToSave.DelayMinutes = delayMinutes;
            recordToSave.IsAbsent = false;
            recordToSave.AbsenceReason = null;

            if (ghostRecord == null)
            {
                _context.TeacherAttendances.Add(recordToSave);
            }
            await _context.SaveChangesAsync();

            return new CheckInResponseDTO
            {
                Success = true,
                Message = "تم تسجيل الحضور بنجاح.",
                DelayMinutes = delayMinutes,
                Record = MapToDTO(recordToSave)
            };
        }

        public async Task<CheckOutResponseDTO> CheckOutAsync(string teacherId)
        {
            var egyptNow = GetEgyptNow();
            var today = DateOnly.FromDateTime(egyptNow);
            var currentTime = egyptNow.TimeOfDay;

            var allTodayRecords = await _context.TeacherAttendances
                .Where(a => a.TeacherId == teacherId && a.Date == today)
                .OrderBy(a => a.Id)
                .ToListAsync();

            // Find the open record that has CheckInTime but no CheckOutTime
            var openRecord = allTodayRecords.FirstOrDefault(a => a.CheckInTime.HasValue && !a.CheckOutTime.HasValue && !a.IsAbsent);

            if (openRecord == null)
            {
                return new CheckOutResponseDTO
                {
                    Success = false,
                    Message = "لم يتم العثور على تسجيل حضور مفتوح لهذه الفترة. يجب تسجيل الحضور أولاً.",
                };
            }

            var user = await _userManager.FindByIdAsync(teacherId);
            if (user == null) return new CheckOutResponseDTO { Success = false, Message = "المستخدم غير موجود." };

            bool bypass = await DoesUserBypassSessionRequirementAsync(teacherId);

            // Allow teacher to check out anytime after check-in without restriction

            openRecord.CheckOutTime = currentTime;
            await _context.SaveChangesAsync();

            return new CheckOutResponseDTO
            {
                Success = true,
                Message = "تم تسجيل الانصراف بنجاح.",
                Record = MapToDTO(openRecord)
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
                if (!record.CheckInTime.HasValue && !record.CheckOutTime.HasValue)
                {
                    _context.TeacherAttendances.Remove(record);
                }
                else
                {
                    record.IsAbsent = false;
                    record.AbsenceReason = null;
                }
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
    
        public async Task<TeacherAttendancePagedResultDTO> GetAttendanceHistoryAsync(
            int page = 1,
            int pageSize = 20,
            string? teacherId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? isAbsent = null,
            bool? hasDelay = null,
            string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.TeacherAttendances
                .Include(a => a.Teacher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(teacherId))
            {
                query = query.Where(a => a.TeacherId == teacherId);
            }

            if (fromDate.HasValue)
            {
                var d = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(a => a.Date >= d);
            }

            if (toDate.HasValue)
            {
                var d = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(a => a.Date <= d);
            }

            if (isAbsent.HasValue)
            {
                query = query.Where(a => a.IsAbsent == isAbsent.Value);
            }

            if (hasDelay.HasValue)
            {
                if (hasDelay.Value)
                    query = query.Where(a => a.DelayMinutes > 0);
                else
                    query = query.Where(a => a.DelayMinutes == 0);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(a =>
                    (a.Teacher != null && (a.Teacher.FullName.Contains(s) || (a.Teacher.PhoneNumber != null && a.Teacher.PhoneNumber.Contains(s)))) ||
                    (a.AbsenceReason != null && a.AbsenceReason.Contains(s)));
            }

            var totalCount = await query.CountAsync();
            var totalAbsences = await query.CountAsync(a => a.IsAbsent);
            var totalLateMinutes = await query.SumAsync(a => a.DelayMinutes);

            var items = await query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CheckInTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dates = items.Select(i => i.Date).Distinct().ToList();
            var teacherIds = items.Select(i => i.TeacherId).Distinct().ToList();

            var sessions = await _context.Sessions
                .Include(s => s.SubstituteTeacher)
                .Include(s => s.Group)
                .Where(s => dates.Contains(s.SessionDate) && teacherIds.Contains(s.Group.TeacherId))
                .ToListAsync();

            var resultItems = items.Select(a =>
            {
                var teacherSessions = sessions
                    .Where(s => s.SessionDate == a.Date && s.Group.TeacherId == a.TeacherId)
                    .ToList();

                var substitutes = teacherSessions
                    .Where(s => !string.IsNullOrEmpty(s.SubstituteTeacherId) && s.SubstituteTeacher != null)
                    .Select(s => s.SubstituteTeacher!.FullName)
                    .Distinct()
                    .ToList();

                return new TeacherAttendanceHistoryItemDTO
                {
                    Id = a.Id,
                    TeacherId = a.TeacherId,
                    TeacherName = a.Teacher?.FullName ?? a.Teacher?.UserName ?? "معلم",
                    PhoneNumber = a.Teacher?.PhoneNumber,
                    Date = DateTime.SpecifyKind(a.Date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    DelayMinutes = a.DelayMinutes,
                    IsAbsent = a.IsAbsent,
                    AbsenceReason = a.AbsenceReason,
                    SessionsCount = teacherSessions.Count,
                    SubstituteTeacherNames = substitutes.Any() ? string.Join("، ", substitutes) : null
                };
            }).ToList();

            return new TeacherAttendancePagedResultDTO
            {
                Items = resultItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalAbsences = totalAbsences,
                TotalLateMinutes = totalLateMinutes
            };
        }

        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = (lat2 - lat1) * (Math.PI / 180.0);
            var dLon = (lon2 - lon1) * (Math.PI / 180.0);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371000.0 * c; // Earth radius in meters
        }
    }
}
