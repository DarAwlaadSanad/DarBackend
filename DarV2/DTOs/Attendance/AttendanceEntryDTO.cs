using DarV2.Models;

namespace DarV2.DTOs
{
    public class AttendanceEntryDTO
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
