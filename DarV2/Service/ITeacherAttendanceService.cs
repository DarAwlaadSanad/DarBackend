using DarV2.DTOs.TeacherAttendance;

namespace DarV2.Service
{
    public interface ITeacherAttendanceService
    {
        Task<TeacherAttendanceRecordDTO?> GetTodayRecordAsync(string teacherId);
        Task<TodayAttendanceStatusDTO> GetTodayAttendanceStatusAsync(string teacherId);
        Task<CheckInResponseDTO> CheckInAsync(string teacherId);
        Task<CheckOutResponseDTO> CheckOutAsync(string teacherId);
        Task<bool> MarkTeacherAbsentAsync(MarkTeacherAbsentDTO dto);
        Task<bool> CancelTeacherAbsentAsync(MarkTeacherAbsentDTO dto);
        Task<List<TeacherMonthlyAttendanceReportDTO>> GetMonthlyReportAsync(int year, int month);
    }
}
