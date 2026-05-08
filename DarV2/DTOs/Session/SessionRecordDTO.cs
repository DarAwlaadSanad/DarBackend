using DarV2.Models;

namespace DarV2.DTOs
{
    public class SessionRecordDTO
    {
        public AttendanceStatus? Attendance { get; set; } 
        public decimal? Score { get; set; } 
        public string? Comment { get; set; }
    }
}
