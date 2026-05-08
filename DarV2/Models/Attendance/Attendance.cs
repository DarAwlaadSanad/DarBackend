namespace DarV2.Models
{
    public class Attendance 
    {
        public int Id { get; set; }
        
        public int SessionId { get; set; }
        public Session Session { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;

        public string? Notes { get; set; }
    }
}
