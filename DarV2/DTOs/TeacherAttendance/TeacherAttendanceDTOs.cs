using System;

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
    public class TodayAttendanceStatusDTO
    {
        public bool HasSessionsToday { get; set; }
        public bool RequiresSessions { get; set; }
        public bool CanCheckIn { get; set; }
        public string? Message { get; set; }
        public TeacherAttendanceRecordDTO? Record { get; set; }
        public int SessionsCount { get; set; }
    }
}