using DarV2.DTOs.TeacherAttendance;

namespace DarV2.Service
{
    public interface ISessionService
    {
        Task<bool> AssignSubstituteAsync(AssignSubstituteDTO dto);
        Task<bool> RevertSubstituteAsync(int sessionId);
        Task<IEnumerable<object>> GetTeacherSessionsByDateAsync(string teacherId, DateOnly date);
        Task<IEnumerable<object>> GetTodaySessionsAsync(string userId, bool isAdmin, bool isTeacher);
    }
}
