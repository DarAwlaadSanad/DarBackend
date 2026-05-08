namespace DarV2.DTOs
{
    public class AttendanceBatchDTO
    {
        public int SessionId { get; set; }
        public List<AttendanceEntryDTO> Entries { get; set; } = new List<AttendanceEntryDTO>();
    }
}
