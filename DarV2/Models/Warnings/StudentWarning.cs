using System;

namespace DarV2.Models
{
    public class StudentWarning
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public WarningType WarningType { get; set; } = WarningType.Absence;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? Reason { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
