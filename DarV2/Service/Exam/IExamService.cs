using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IExamService
    {
        Task<ExamDTO> CreateExamAsync(CreateExamDTO dto);
        Task<IEnumerable<ExamDTO>> GetExamsByGroupAsync(int groupId);
        Task<ExamDTO?> GetExamByIdAsync(int id);
        Task<bool> DeleteExamAsync(int id);
        
        Task<IEnumerable<ExamResultDTO>> GetExamResultsAsync(int examId);
        Task<bool> SaveExamResultsAsync(int examId, SaveExamResultsDTO dto);
        
        Task<IEnumerable<ExamResultDTO>> GetStudentExamResultsAsync(int studentId);
    }
}
