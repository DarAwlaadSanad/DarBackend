namespace DarV2.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? SSN { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        public string Code { get; set; } = string.Empty; // كود الطالب مثلاً STD-001
        public string PasswordHash { get; set; } = string.Empty;

        public int? AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; }
        //public int? SurahId { get; set; }
        //public Surah Surah { get; set; }


        public ICollection<MemorizationRecord> MemorizationRecords { get; set; } = new List<MemorizationRecord>();

        // Many-to-Many with Groups
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();

        // Attendance & Evaluation records
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();

        public ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
    }

}
