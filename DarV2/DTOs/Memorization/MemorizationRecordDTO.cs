namespace DarV2.DTOs
{
    public class MemorizationRecordDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int FromSurahId { get; set; }
        public int FromAyah { get; set; }
        public int ToSurahId { get; set; }
        public int ToAyah { get; set; }
        public DateOnly Date { get; set; }
        public string? Notes { get; set; }
    }
}
