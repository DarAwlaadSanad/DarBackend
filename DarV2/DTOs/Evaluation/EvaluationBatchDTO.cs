namespace DarV2.DTOs
{
    public class EvaluationBatchDTO
    {
        public int SessionId { get; set; }
        public List<EvaluationEntryDTO> Entries { get; set; } = new List<EvaluationEntryDTO>();
    }
}
