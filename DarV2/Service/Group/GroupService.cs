using DarV2.DTOs;
using DarV2.Models;
using DarV2.Repository;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _uow;

        public GroupService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<GroupCardDTO>> GetAllAsync()
        {
            var groups = await _uow.Groups.GetAllWithIncludes();
            return groups.Select(g => new GroupCardDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                TeacherId = g.TeacherId,
                TeacherName = g.Teacher?.FullName ?? g.Teacher?.UserName,
                StudentCount = g.StudentGroups?.Count ?? 0,
                MaleCount = g.StudentGroups?.Count(sg => sg.Student?.Gender == Gender.Male) ?? 0,
                FemaleCount = g.StudentGroups?.Count(sg => sg.Student?.Gender == Gender.Female) ?? 0,
                IsOnline = g.IsOnline,
                RoomId = g.RoomId,
                RoomName = g.Room?.Name
            }).ToList();
        }

        public async Task<GroupDetailsDTO?> GetByIdAsync(int groupId, int month, int year)
        {
            var group = await (_uow.Groups as IGroupRepository)?.GetGroupWithDetailsAsync(groupId);
            if (group == null) return null;

            // 2. جيب الحصص في الشهر ده
            var sessions = await _uow.Sessions
                .GetSessionsByGroupAndMonthAsync(groupId, month, year);

            // 3. جيب الطلاب النشطين في المجموعة
            var students = await _uow.Students.GetStudentsGroupAsync(groupId);
            var sessionIds = sessions.Select(s => s.Id).ToList();

            var allAttendances = await _uow.Attendances.FindAsync(
                a => sessionIds.Contains(a.SessionId));

            var allEvaluations = await _uow.Evaluations.FindAsync(
                e => sessionIds.Contains(e.SessionId));

            // 5. ابني الـ pivot (Dictionary للـ O(1) access)
            var attMap = allAttendances
                .GroupBy(a => (a.SessionId, a.StudentId))
                .ToDictionary(g => g.Key, g => g.First());

            var evalMap = allEvaluations
                .GroupBy(e => (e.SessionId, e.StudentId))
                .ToDictionary(g => g.Key, g => g.First());

            // 6. ركّب الـ DTO
            return new GroupDetailsDTO
            {
                GroupId = group.Id,
                MaleCount = students.Count(s => s.Gender == Gender.Male),
                FemaleCount = students.Count(s => s.Gender == Gender.Female),
                GroupName = group.Name,
                TeacherName = group.Teacher?.FullName ?? "لا يوجد معلم",
                Month = month,
                Year = year,
                IsOnline = group.IsOnline,
                RoomId = group.RoomId,
                RoomName = group.Room?.Name,

                Sessions = sessions.Select(s => new SessionViewDTO
                {
                    SessionId = s.Id,
                    Date = s.SessionDate,
                    StartTime = s.StartTime,
                }).ToList(),

                Students = students.Select(student =>
                {
                    var records = sessions.ToDictionary(
                        s => s.Id,
                        s => new SessionRecordDTO
                        {
                            Attendance = attMap.TryGetValue((s.Id, student.Id), out var att)
                                         ? att.Status : null,
                            Score = evalMap.TryGetValue((s.Id, student.Id), out var ev)
                                         ? ev.Score : null,
                            Comment = ev?.Comment
                        });

                    var totalEvaluation = evalMap.Where(r => r.Key.StudentId == student.Id).Sum(r => r.Value.Score);

                    var presentCount = records.Values
                        .Count(r => r.Attendance is AttendanceStatus.Present
                                                or AttendanceStatus.Late);

                    return new StudentInGroupDTO
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        Gender = student.Gender,
                        Records = records,
                        TotalPresent = presentCount,
                        TotalEvaluation = totalEvaluation??0,
                    };
                }).ToList()
            };
        }
        public async Task<Models.Group?> CreateAsync(GroupAddDTO dto)
        {
            if (!dto.IsOnline && dto.RoomId.HasValue)
            {
                var roomExists = await _uow.Rooms.GetByIdAsync(dto.RoomId.Value);
                if (roomExists == null) throw new Exception("الغرفة المحددة غير موجودة");
            }

            var group = new Models.Group
            {
                Name = dto.Name,
                Description = dto.Description,
                TeacherId = dto.TeacherId,
                IsOnline = dto.IsOnline,
                RoomId = dto.RoomId
            };

            await _uow.Groups.AddAsync(group);
            await _uow.SaveAsync();
            return group;
        }

        public async Task<bool> UpdateAsync(int id, GroupAddDTO dto)
        {
            var group = await _uow.Groups.GetByIdAsync(id);
            if (group == null) return false;

            if (!dto.IsOnline && dto.RoomId.HasValue)
            {
                var roomExists = await _uow.Rooms.GetByIdAsync(dto.RoomId.Value);
                if (roomExists == null) throw new Exception("الغرفة المحددة غير موجودة");

                // Check active schedules of this group for room conflicts
                var mySchedules = await _uow.GroupSchedules.FindAsync(gs => gs.GroupId == id && gs.IsActive);
                if (mySchedules.Any())
                {
                    foreach (var sched in mySchedules)
                    {
                        var conflicts = await _uow.GroupSchedules.Query()
                            .Include(gs => gs.Group)
                            .Where(gs => gs.IsActive 
                                      && gs.DayOfWeek == sched.DayOfWeek 
                                      && gs.GroupId != id
                                      && gs.Group != null 
                                      && gs.Group.RoomId == dto.RoomId)
                            .ToListAsync();

                        if (conflicts.Any(c => sched.StartTime < c.EndTime && sched.EndTime > c.StartTime))
                        {
                            throw new Exception("لا يمكن تعيين المجموعة لهذه الغرفة بسبب تعارض مواعيد إحدى حصصها مع مجموعة أخرى في نفس الغرفة.");
                        }
                    }
                }
            }

            group.Name = dto.Name;
            group.Description = dto.Description;
            group.TeacherId = dto.TeacherId;
            group.IsOnline = dto.IsOnline;
            group.RoomId = dto.RoomId;

            _uow.Groups.Update(group);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await _uow.Groups.Query()
                .Include(g => g.Schedules)
                .Include(g => g.StudentGroups)
                .Include(g => g.Sessions)
                .Include(g => g.FeePlans)
                .Include(g => g.StudentFees)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return false;

            // Clear related records to satisfy foreign key constraints
            if (group.Schedules != null && group.Schedules.Any()) _uow.GroupSchedules.RemoveRange(group.Schedules);
            if (group.StudentGroups != null && group.StudentGroups.Any()) _uow.StudentGroups.RemoveRange(group.StudentGroups);
            if (group.Sessions != null && group.Sessions.Any()) _uow.Sessions.RemoveRange(group.Sessions);
            if (group.FeePlans != null && group.FeePlans.Any()) _uow.FeePlans.RemoveRange(group.FeePlans);
            if (group.StudentFees != null && group.StudentFees.Any()) _uow.StudentFees.RemoveRange(group.StudentFees);

            _uow.Groups.Remove(group);
            await _uow.SaveAsync();
            return true;
        }
    }
}
