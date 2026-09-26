using DarV2.Models;
using System;

namespace DarV2.Models
{
    public class TeacherAttendance
    {
        public int Id { get; set; }
        
        public string TeacherId { get; set; } = string.Empty;
        public ApplicationUser? Teacher { get; set; }
        
        public DateOnly Date { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public int DelayMinutes { get; set; }
        
        // الغياب
        public bool IsAbsent { get; set; } = false;
        public string? AbsenceReason { get; set; }
    }
}
