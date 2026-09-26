namespace DarV2.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? SSN { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public Gender? Gender { get; set; }

        public string Code { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Permanent fee exemption: if true, all generated fees for this student are auto-exempted
        public bool IsFeeExempted { get; set; } = false;
        public string? FeeExemptionReason { get; set; }

        public int? AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; }

        public ICollection<MemorizationRecord> MemorizationRecords { get; set; } = new List<MemorizationRecord>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
        public ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
        public ICollection<StudentWarning> Warnings { get; set; } = new List<StudentWarning>();
    }
}
