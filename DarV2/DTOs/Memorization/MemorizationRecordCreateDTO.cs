namespace DarV2.DTOs
{
    public class MemorizationRecordCreateDTO
    {
        public int StudentId { get; set; }
        public int FromSurahId { get; set; }
        public int FromAyah { get; set; }
        public int ToSurahId { get; set; }
        public int ToAyah { get; set; }
        public DateOnly Date { get; set; }
        public string? Notes { get; set; }
    }
}
