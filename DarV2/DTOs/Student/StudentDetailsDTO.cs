namespace DarV2.DTOs
{
    public class StudentDetailsDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? SSN { get; set; }
        public bool IsActive { get; set; }
        public string Code { get; set; }
        public string? Notes { get; set; }
        public AcademicYearViewDTO AcademicYear { get; set; }
        public List<MemorizationRecordDTO> MemorizationRecords { get; set; }
        public List<GroupCardDTO> Groups { get; set; }
        public List<PhoneViewDTO> Phones { get; set; }
        public List<ImageViewDTO> Images { get; set; }
        
    }
}
