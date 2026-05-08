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
            await _uow.BeginTransactionAsync();
            try
            {
                // 1. لو في جدول قديم نشط → اقفله
                var existing = await _uow.GroupSchedules.FindAsync(
                    gs => gs.GroupId == dto.GroupId
                       && gs.DayOfWeek == dto.DayOfWeek
                       && gs.StartTime == dto.StartTime
                       && gs.IsActive);

                foreach (var old in existing)
                {
                    //old.IsActive = false;
                    //old.EffectiveTo = dto.EffectiveFrom.AddDays(-1);
                    //_uow.GroupSchedules.Update(old);
                    RemoveAsync(old.Id);
                }

                // 2. أضف الجدول الجديد
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
                await _uow.SaveAsync(); // عشان يبقى ليه Id

                // 3. ولّد Sessions للشهور القادمة
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

        // ─── Core generation logic ────────────────────────────────────────────────
        private async Task GenerateSessionsFromScheduleAsync(
            GroupSchedule schedule,
            int monthsAhead
            )
        {
            //var today = DateOnly.FromDateTime(DateTime.Today);
            var today = DateOnly.FromDateTime(schedule.EffectiveFrom.ToDateTime(TimeOnly.MinValue));

            // ابدأ من EffectiveFrom أو من أول الشهر الحالي (الأكبر)
            var startDate = schedule.EffectiveFrom > today
                ? schedule.EffectiveFrom
                : today;

            // انتهي بعد monthsAhead شهر
            var endDate = today.AddMonths(monthsAhead);
            endDate = new DateOnly(endDate.Year, endDate.Month,
                          DateTime.DaysInMonth(endDate.Year, endDate.Month));

            // لو في EffectiveTo → احترمه
            if (schedule.EffectiveTo.HasValue && schedule.EffectiveTo < endDate)
                endDate = schedule.EffectiveTo.Value;

            var sessionsToAdd = new List<Session>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // هل اليوم ده مطابق ليوم الجدول؟
                if ((DayOfWeekAr)date.DayOfWeek != schedule.DayOfWeek)
                    continue;

                // تحقق مش موجود قبل كده (نفس المجموعة + نفس التاريخ + نفس الوقت)
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
