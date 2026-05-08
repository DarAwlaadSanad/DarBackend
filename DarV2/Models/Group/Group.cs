namespace DarV2.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Teacher
        public string? TeacherId { get; set; }
        public ApplicationUser? Teacher { get; set; } 


        public ICollection<FeePlan> FeePlans { get; set; } = new List<FeePlan>();
        public ICollection<StudentFee> StudentFees { get; set; }

        // مواعيد الحصص الأسبوعية الثابتة للمجموعة
        public ICollection<GroupSchedule> Schedules { get; set; } = new List<GroupSchedule>();

        // الحصص الفعلية (مولّدة أو يدوية)
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        // الطلاب في المجموعة
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
    }
}
