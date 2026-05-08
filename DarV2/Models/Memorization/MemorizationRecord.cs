namespace DarV2.Models
{
    public class MemorizationRecord
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int FromSurahId { get; set; }
        public int FromAyah { get; set; }

        public int ToSurahId { get; set; }
        public int ToAyah { get; set; }

        public DateOnly Date { get; set; }

        public string? Notes { get; set; }
    }
}
