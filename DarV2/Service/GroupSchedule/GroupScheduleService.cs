using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class GroupScheduleService : IGroupScheduleService
    {
        private readonly IUnitOfWork _uow;

        public GroupScheduleService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<GroupSchedule> AddAsync(CreateGroupScheduleDTO dto)
        {
            if (dto.EndTime <= dto.StartTime)
            {
                throw new Exception("ÙˆÙ‚Øª Ø§Ù„Ø§Ù†ØªÙ‡Ø§Ø¡ ÙŠØ¬Ø¨ Ø£Ù† ÙŠÙƒÙˆÙ† Ø¨Ø¹Ø¯ ÙˆÙ‚Øª Ø§Ù„Ø¨Ø¯Ø¡");
            }

            await _uow.BeginTransactionAsync();
            try
            {
                var group = await _uow.Groups.GetByIdAsync(dto.GroupId);
                if (group != null && !group.IsOnline && group.RoomId.HasValue)
                {
                    // Check for room overlap conflicts
                    var conflicts = await _uow.GroupSchedules.Query()
                        .Include(gs => gs.Group)
                        .Where(gs => gs.IsActive 
                                  && gs.DayOfWeek == dto.DayOfWeek 
                                  && gs.GroupId != dto.GroupId
                                  && gs.Group != null 
                                  && gs.Group.RoomId == group.RoomId)
                        .ToListAsync();

                    var conflict = conflicts.FirstOrDefault(c => dto.StartTime < c.EndTime && dto.EndTime > c.StartTime);
                    if (conflict != null)
                    {
                        var conflictGroupName = conflict.Group?.Name ?? "Ù…Ø¬Ù…ÙˆØ¹Ø© Ø£Ø®Ø±Ù‰";
                        var startStr = conflict.StartTime.ToString(@"hh\:mm");
                        var endStr = conflict.EndTime.ToString(@"hh\:mm");
                        throw new Exception($"ØªØ¹Ø§Ø±Ø¶ ÙÙŠ Ø§Ù„Ù…ÙˆØ¹Ø¯: Ø§Ù„ØºØ±ÙØ© Ù…Ø­Ø¬ÙˆØ²Ø© Ù„Ù€ ({conflictGroupName}) ÙÙŠ Ù†ÙØ³ Ø§Ù„ÙŠÙˆÙ… Ù…Ù† {startStr} Ø¥Ù„Ù‰ {endStr}. Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø§Ù„Ø¥Ø¶Ø§ÙØ© Ø¥Ù„Ø§ Ø¨Ø¹Ø¯ Ø§Ù†ØªÙ‡Ø§Ø¡ Ø§Ù„Ù…ÙˆØ¹Ø¯ Ø§Ù„Ø£ÙˆÙ„.");
                    }
                }

                // 1. Ù„Ùˆ ÙÙŠ Ø¬Ø¯ÙˆÙ„ Ù‚Ø¯ÙŠÙ… Ù†Ø´Ø· â†’ Ø§Ù‚ÙÙ„Ù‡
                var existing = await _uow.GroupSchedules.FindAsync(
                    gs => gs.GroupId == dto.GroupId
                       && gs.DayOfWeek == dto.DayOfWeek
                       && gs.StartTime == dto.StartTime
                       && gs.IsActive);

                foreach (var old in existing)
                {
                    await RemoveAsync(old.Id);
                }

                // 2. Ø£Ø¶Ù Ø§Ù„Ø¬Ø¯ÙˆÙ„ Ø§Ù„Ø¬Ø¯ÙŠØ¯
                var schedule = new Models.GroupSchedule
                {
                    GroupId = dto.GroupId,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    EffectiveFrom = dto.EffectiveFrom,
                    IsActive = true
                };
                await _uow.GroupSchedules.AddAsync(schedule);
                await _uow.SaveAsync(); // Ø¹Ø´Ø§Ù† ÙŠØ¨Ù‚Ù‰ Ù„ÙŠÙ‡ Id

                // 3. ÙˆÙ„Ù‘Ø¯ Sessions Ù„Ù„Ø´Ù‡ÙˆØ± Ø§Ù„Ù‚Ø§Ø¯Ù…Ø©
                await GenerateSessionsFromScheduleAsync(schedule, monthsAhead: 5);

                await _uow.SaveAsync();
                await _uow.CommitAsync();

                return schedule;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        // â”€â”€â”€ Core generation logic â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task GenerateSessionsFromScheduleAsync(
            GroupSchedule schedule,
            int monthsAhead
            )
        {
            //var today = DateOnly.FromDateTime(DateTime.Today);
            var today = DateOnly.FromDateTime(schedule.EffectiveFrom.ToDateTime(TimeOnly.MinValue));

            // Ø§Ø¨Ø¯Ø£ Ù…Ù† EffectiveFrom Ø£Ùˆ Ù…Ù† Ø£ÙˆÙ„ Ø§Ù„Ø´Ù‡Ø± Ø§Ù„Ø­Ø§Ù„ÙŠ (Ø§Ù„Ø£ÙƒØ¨Ø±)
            var startDate = schedule.EffectiveFrom > today
                ? schedule.EffectiveFrom
                : today;

            // Ø§Ù†ØªÙ‡ÙŠ Ø¨Ø¹Ø¯ monthsAhead Ø´Ù‡Ø±
            var endDate = today.AddMonths(monthsAhead);
            endDate = new DateOnly(endDate.Year, endDate.Month,
                          DateTime.DaysInMonth(endDate.Year, endDate.Month));

            // Ù„Ùˆ ÙÙŠ EffectiveTo â†’ Ø§Ø­ØªØ±Ù…Ù‡
            if (schedule.EffectiveTo.HasValue && schedule.EffectiveTo < endDate)
                endDate = schedule.EffectiveTo.Value;

            var sessionsToAdd = new List<Session>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Ù‡Ù„ Ø§Ù„ÙŠÙˆÙ… Ø¯Ù‡ Ù…Ø·Ø§Ø¨Ù‚ Ù„ÙŠÙˆÙ… Ø§Ù„Ø¬Ø¯ÙˆÙ„ØŸ
                if ((DayOfWeekAr)date.DayOfWeek != schedule.DayOfWeek)
                    continue;

                // ØªØ­Ù‚Ù‚ Ù…Ø´ Ù…ÙˆØ¬ÙˆØ¯ Ù‚Ø¨Ù„ ÙƒØ¯Ù‡ (Ù†ÙØ³ Ø§Ù„Ù…Ø¬Ù…ÙˆØ¹Ø© + Ù†ÙØ³ Ø§Ù„ØªØ§Ø±ÙŠØ® + Ù†ÙØ³ Ø§Ù„ÙˆÙ‚Øª)
                var exists = await _uow.Sessions.SessionExistsAsync(
                    schedule.GroupId, date, schedule.StartTime);

                if (exists) continue;

                sessionsToAdd.Add(new Session
                {
                    GroupId = schedule.GroupId,
                    GroupScheduleId = schedule.Id,
                    SessionDate = date,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                });
            }

            if (sessionsToAdd.Any())
                await _uow.Sessions.AddRangeAsync(sessionsToAdd);
        }

        public async Task<IEnumerable<GroupScheduleViewDTO>> GetByGroupAsync(int groupId)
        {
            var items = await _uow.GroupSchedules.FindAsync(gs => gs.GroupId == groupId);
            return items.Select(i => new GroupScheduleViewDTO
            {
                Id = i.Id,
                GroupId = i.GroupId,
                DayOfWeek = (int)i.DayOfWeek,
                StartTime = i.StartTime,
                EndTime = i.EndTime,
                EffectiveFrom = i.EffectiveFrom,
                EffectiveTo = i.EffectiveTo,
                IsActive = i.IsActive
            }).ToList();
        }

        public async Task<IEnumerable<WeeklyScheduleItemDTO>> GetAllActiveWeeklySchedulesAsync()
        {
            var items = await _uow.GroupSchedules.Query()
                .Include(gs => gs.Group)
                    .ThenInclude(g => g.Teacher)
                .Include(gs => gs.Group)
                    .ThenInclude(g => g.Room)
                .Where(gs => gs.IsActive)
                .ToListAsync();

            return items.Select(i => new WeeklyScheduleItemDTO
            {
                ScheduleId = i.Id,
                GroupId = i.GroupId,
                GroupName = i.Group?.Name ?? "Ø¨Ø¯ÙˆÙ† Ø§Ø³Ù…",
                TeacherId = i.Group?.TeacherId,
                TeacherName = i.Group?.Teacher?.FullName ?? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯",
                DayOfWeek = (int)i.DayOfWeek,
                StartTime = i.StartTime,
                EndTime = i.EndTime,
                EffectiveFrom = i.EffectiveFrom,
                EffectiveTo = i.EffectiveTo,
                IsOnline = i.Group?.IsOnline ?? false,
                RoomName = i.Group?.Room?.Name
            });
        }

                        public async Task<byte[]> ExportWeeklySchedulePdfAsync(string? teacherId = null)
        {
            var schedules = await GetAllActiveWeeklySchedulesAsync();
            string? teacherName = null;

            if (!string.IsNullOrWhiteSpace(teacherId) && teacherId != "all")
            {
                schedules = schedules.Where(s => s.TeacherId == teacherId).ToList();
                teacherName = schedules.FirstOrDefault(s => s.TeacherId == teacherId)?.TeacherName;
            }

            return DarV2.Service.Export.SchedulePdfGenerator.GenerateWeeklySchedulePdf(schedules, teacherName);
        }

        public async Task<bool> RemoveAsync(int scheduleId)
        {
            var schedule = await _uow.GroupSchedules.GetByIdAsync(scheduleId);
            if (schedule == null) return false;

            // deactivate schedule
            schedule.IsActive = false;
            schedule.EffectiveTo = DateOnly.FromDateTime(DateTime.Today);
            _uow.GroupSchedules.Update(schedule);

            
            // find next session generated from this schedule that is in future and has no attendances/evaluations
            var next = await _uow.Sessions.Query()
                .Where(s => s.GroupScheduleId == scheduleId && s.SessionDate >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(s => s.SessionDate).ToListAsync();
                    

            if (next != null)
            {
                // ensure no attendance/evaluation exists for that session
               foreach(var n in next)
                {
                    var hasAttendance = (await _uow.Attendances.FindAsync(a => a.SessionId == n.Id)).Any();
                    var hasEval = (await _uow.Evaluations.FindAsync(e => e.SessionId == n.Id)).Any();

                    if (!hasAttendance && !hasEval)
                    {
                        _uow.Sessions.Remove(n);
                    }
                }
            }
            

            await _uow.SaveAsync();
            return true;
        }
    }
}


