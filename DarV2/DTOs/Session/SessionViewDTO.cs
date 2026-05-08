namespace DarV2.DTOs
{
    public class SessionViewDTO
    {
        public int SessionId { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
    }
}
