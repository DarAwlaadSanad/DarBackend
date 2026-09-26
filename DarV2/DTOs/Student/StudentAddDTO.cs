namespace DarV2.DTOs
{
    public class StudentAddDTO
    {
        public string FullName { get; set; }
        public string SSN { get; set; }
        public string? Notes { get; set; }
        public Models.Gender? Gender { get; set; }
        public int AcademicYearId { get; set; }
        public List<int> GroupIds { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>();
        public List<string>? PhoneNumbers { get; set; } = new List<string>();
    }
}
