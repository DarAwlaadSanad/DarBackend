using DarV2.DTOs;

namespace DarV2.Service
{
    public interface ICompetitionService
    {
        Task<CompetitionDTO> CreateCompetitionAsync(CreateCompetitionDTO dto);
        Task<IEnumerable<CompetitionDTO>> GetAllCompetitionsAsync();
        Task<CompetitionDTO?> GetCompetitionByIdAsync(int id);
        Task<bool> DeleteCompetitionAsync(int id);

        Task<CompetitionLevelDTO> CreateLevelAsync(CreateCompetitionLevelDTO dto);
        Task<bool> DeleteLevelAsync(int levelId);

        Task<bool> RegisterStudentToLevelAsync(RegisterStudentDTO dto);
        Task<bool> UnregisterStudentFromLevelAsync(int levelId, int studentId);
        Task<IEnumerable<CompetitionResultDTO>> GetLevelResultsAsync(int levelId);
        Task<bool> SaveLevelResultsAsync(int levelId, SaveCompetitionResultsDTO dto);
        Task<IEnumerable<StudentCompetitionResultDTO>> GetStudentCompetitionsAsync(int studentId);
    }
}
