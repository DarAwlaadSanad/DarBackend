namespace DarV2.DTOs
{
    public class StudentInGroupDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public Models.Gender? Gender { get; set; }
        public Dictionary<int, SessionRecordDTO> Records { get; set; } = new();
        public int TotalPresent { get; set; }
        public decimal TotalEvaluation { get; set; }
    }
}
