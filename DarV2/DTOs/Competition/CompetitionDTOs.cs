using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DarV2.DTOs
{
    public class CompetitionDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly Date { get; set; }
        public string? Notes { get; set; }
        public List<CompetitionLevelDTO> Levels { get; set; } = new List<CompetitionLevelDTO>();
    }

    public class CreateCompetitionDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        public DateOnly Date { get; set; }
        public string? Notes { get; set; }
    }

    public class CompetitionLevelDTO
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public string Name { get; set; }
        public decimal MaxScore { get; set; }
        public int RegisteredStudentsCount { get; set; }
    }

    public class CreateCompetitionLevelDTO
    {
        public int CompetitionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public decimal MaxScore { get; set; }
    }

    public class CompetitionResultDTO
    {
        public int Id { get; set; }
        public int CompetitionLevelId { get; set; }
        public string LevelName { get; set; }
        public decimal MaxScore { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
    }

    public class RegisterStudentDTO
    {
        public int CompetitionLevelId { get; set; }
        public int StudentId { get; set; }
    }

    public class SaveCompetitionResultsDTO
    {
        public List<UpdateCompetitionResultDTO> Results { get; set; } = new List<UpdateCompetitionResultDTO>();
    }

    public class UpdateCompetitionResultDTO
    {
        public int StudentId { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
    }

    public class StudentCompetitionResultDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly Date { get; set; }
        public string LevelName { get; set; }
        public decimal MaxScore { get; set; }
        public decimal? Score { get; set; }
        public string? Notes { get; set; }
    }
}
