using System.ComponentModel.DataAnnotations;

namespace DarV2.Models
{
    public class Exam
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public DateOnly Date { get; set; }

        public decimal MaxScore { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public string? Notes { get; set; }

        public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
