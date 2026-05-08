namespace DarV2.DTOs
{
    public class StudentPagedResultDTO
    {
        public IEnumerable<StudentDetailsDTO> Items { get; set; } = new List<StudentDetailsDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
