using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarV2.Models
{
    public class CompetitionLevel
    {
        public int Id { get; set; }

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public decimal MaxScore { get; set; }

        public ICollection<CompetitionResult> Results { get; set; } = new List<CompetitionResult>();
    }
}
