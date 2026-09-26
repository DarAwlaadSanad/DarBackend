namespace DarV2.DTOs
{
    public class StudentUpdateDTO
    {
        public string FullName { get; set; }
        public string? SSN { get; set; }
        public string? Notes { get; set; }
        public Models.Gender? Gender { get; set; }
        public int? AcademicYearId { get; set; }
    }
}
