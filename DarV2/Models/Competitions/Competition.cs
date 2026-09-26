using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarV2.Models
{
    public class Competition
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public DateOnly Date { get; set; }

        public string? Notes { get; set; }

        public ICollection<CompetitionLevel> Levels { get; set; } = new List<CompetitionLevel>();
    }
}
