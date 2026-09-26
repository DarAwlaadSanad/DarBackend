using System.ComponentModel.DataAnnotations;

namespace DarV2.Models
{
    public class CompetitionResult
    {
        public int Id { get; set; }

        public int CompetitionLevelId { get; set; }
        public CompetitionLevel CompetitionLevel { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public decimal? Score { get; set; }

        public string? Notes { get; set; }
    }
}
