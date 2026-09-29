using DarV2.DTOs.TeacherAttendance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarV2.Service
{
    public interface ITeacherAttendanceService
    {
        Task<TeacherAttendanceRecordDTO?> GetTodayRecordAsync(string teacherId);
        Task<TodayAttendanceStatusDTO> GetTodayAttendanceStatusAsync(string teacherId);
        Task<CheckInResponseDTO> CheckInAsync(string teacherId, double? latitude = null, double? longitude = null);
        Task<CheckOutResponseDTO> CheckOutAsync(string teacherId);
        Task<bool> MarkTeacherAbsentAsync(MarkTeacherAbsentDTO dto);
        Task<bool> CancelTeacherAbsentAsync(MarkTeacherAbsentDTO dto);
        Task<List<TeacherMonthlyAttendanceReportDTO>> GetMonthlyReportAsync(int year, int month);
        Task<TeacherAttendancePagedResultDTO> GetAttendanceHistoryAsync(
            int page = 1,
            int pageSize = 20,
            string? teacherId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? isAbsent = null,
            bool? hasDelay = null,
            string? search = null);
    }
}
