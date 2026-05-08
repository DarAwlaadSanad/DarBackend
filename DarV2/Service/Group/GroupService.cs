using DarV2.DTOs;
using DarV2.Models;
using DarV2.Repository;
using DarV2.UnitofWork;

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
                TeacherName = g.Teacher?.UserName,
                StudentCount = g.StudentGroups?.Count ?? 0
            }).ToList();
        }

        public async Task<GroupDetailsDTO?> GetByIdAsync(int groupId, int month, int year)
        {
            var group = await (_uow.Groups as IGroupRepository)?.GetGroupWithDetailsAsync(groupId);

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
                GroupName = group.Name,
                TeacherName = group.Teacher.FullName,
                Month = month,
                Year = year,

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
                        Records = records,
                        TotalPresent = presentCount,
                        TotalEvaluation = totalEvaluation??0,
                    };
                }).ToList()
            };
        }
        public async Task<Models.Group?> CreateAsync(GroupAddDTO dto)
        {
            var group = new Models.Group
            {
                Name = dto.Name,
                Description = dto.Description,
                TeacherId = dto.TeacherId
            };

            await _uow.Groups.AddAsync(group);
            await _uow.SaveAsync();
            return group;
        }

        public async Task<bool> UpdateAsync(int id, GroupAddDTO dto)
        {
            var group = await _uow.Groups.GetByIdAsync(id);
            if (group == null) return false;

            group.Name = dto.Name;
            group.Description = dto.Description;
            group.TeacherId = dto.TeacherId;

            _uow.Groups.Update(group);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await _uow.Groups.GetByIdAsync(id);
            if (group == null) return false;

            _uow.Groups.Remove(group);
            await _uow.SaveAsync();
            return true;
        }
    }
}
