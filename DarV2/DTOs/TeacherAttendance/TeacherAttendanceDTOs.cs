using System;
using System.Collections.Generic;

namespace DarV2.DTOs.TeacherAttendance
{
    public class TeacherAttendanceRecordDTO
    {
        public int? Id { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public DateTime Date { get; set; } // Using DateTime for simpler JSON serialization, mapped from DateOnly
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public int DelayMinutes { get; set; }
        public bool IsAbsent { get; set; }
        public string? AbsenceReason { get; set; }
    }

    public class TeacherAttendanceHistoryItemDTO
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public int DelayMinutes { get; set; }
        public bool IsAbsent { get; set; }
        public string? AbsenceReason { get; set; }
        public int SessionsCount { get; set; }
        public string? SubstituteTeacherNames { get; set; }
    }

    public class TeacherAttendancePagedResultDTO
    {
        public IEnumerable<TeacherAttendanceHistoryItemDTO> Items { get; set; } = new List<TeacherAttendanceHistoryItemDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
        public int TotalAbsences { get; set; }
        public int TotalLateMinutes { get; set; }
    }

    public class MarkTeacherAbsentDTO
    {
        public string TeacherId { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string? Reason { get; set; }
    }

    public class AssignSubstituteDTO
    {
        public int SessionId { get; set; }
        public string SubstituteTeacherId { get; set; } = string.Empty;
    }

    public class CheckInResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int DelayMinutes { get; set; }
        public TeacherAttendanceRecordDTO? Record { get; set; }
    }

    public class CheckOutResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TeacherAttendanceRecordDTO? Record { get; set; }
    }

    public class TeacherMonthlyAttendanceReportDTO
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int AbsentDays { get; set; }
        public int TotalLateMinutes { get; set; }
        public int AbsentSessions { get; set; }
        public int LateSessions { get; set; }
    }

    public class TeacherPeriodDTO
    {
        public int PeriodNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SessionsCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TodayAttendanceStatusDTO
    {
        public bool HasSessionsToday { get; set; }
        public bool RequiresSessions { get; set; }
        public bool CanCheckIn { get; set; }
        public bool IsCheckInOpen { get; set; } = true;
        public string? AllowedCheckInTime { get; set; }
        public int? SecondsUntilCheckIn { get; set; }
        public string? Message { get; set; }
        public TeacherAttendanceRecordDTO? Record { get; set; }
        public int SessionsCount { get; set; }
        public List<TeacherPeriodDTO> Periods { get; set; } = new();
        public List<TeacherAttendanceRecordDTO> AllTodayRecords { get; set; } = new();
        public int CurrentPeriodNumber { get; set; } = 1;
    }
}

    public class CheckInRequestDTO
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
