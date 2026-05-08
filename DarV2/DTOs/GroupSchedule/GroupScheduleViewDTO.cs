namespace DarV2.DTOs
{
    public class GroupScheduleViewDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }
}