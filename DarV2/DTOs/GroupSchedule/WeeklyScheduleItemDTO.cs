namespace DarV2.DTOs
{
    public class WeeklyScheduleItemDTO
    {
        public int ScheduleId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int DayOfWeek { get; set; } // 0 = Saturday, ..., 6 = Friday (DayOfWeekAr)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsOnline { get; set; }
        public string? RoomName { get; set; }
    }
}
