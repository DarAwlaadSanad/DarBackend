namespace DarV2.Models
{
    public class Session 
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

        // المصدر: هل تم توليدها من جدول ثابت؟
        public int? GroupScheduleId { get; set; }
        public GroupSchedule? GroupSchedule { get; set; }

        public DateOnly SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // المدرس البديل (في حالة غياب المدرس الأساسي)
        public string? SubstituteTeacherId { get; set; }
        public ApplicationUser? SubstituteTeacher { get; set; }

        // الحضور والغياب لهذه الحصة
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        // التقييمات لهذه الحصة
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
    }

}
