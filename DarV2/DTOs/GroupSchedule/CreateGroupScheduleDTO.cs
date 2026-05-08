using DarV2.Models;

namespace DarV2.DTOs
{
    public class CreateGroupScheduleDTO
    {
        public int GroupId { get; set; }
        public DayOfWeekAr DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly EffectiveFrom { get; set; }
    }
}

