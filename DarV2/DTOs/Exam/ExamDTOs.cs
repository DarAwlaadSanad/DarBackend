using System.ComponentModel.DataAnnotations;

namespace DarV2.DTOs
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly Date { get; set; }
        public decimal MaxScore { get; set; }
        public int GroupId { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateExamDTO
    {
        [Required]
        public string Title { get; set; }
        public DateOnly Date { get; set; }
        public decimal MaxScore { get; set; }
        public int GroupId { get; set; }
        public string? Notes { get; set; }
    }

    public class ExamResultDTO
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string? ExamTitle { get; set; }
        public DateOnly? ExamDate { get; set; }
        public decimal? MaxScore { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
    }

    public class SaveExamResultsDTO
    {
        public List<UpdateExamResultDTO> Results { get; set; } = new List<UpdateExamResultDTO>();
    }

    public class UpdateExamResultDTO
    {
        public int StudentId { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
    }
}
