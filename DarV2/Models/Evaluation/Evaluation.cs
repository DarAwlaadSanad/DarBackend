namespace DarV2.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        // تقييم كمّي (0-100) أو Grade
        public decimal? Score { get; set; }

        // تقييم نصي / ملاحظة المدرس
        public string? Comment { get; set; }
    }

}
