namespace DarV2.Models
{
    public class GroupSchedule
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

        public DayOfWeekAr DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // تاريخ بداية ونهاية تطبيق هذا الجدول (لو اتغير الجدول)
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
