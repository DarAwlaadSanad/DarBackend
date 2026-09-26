using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DarV2.DTOs;
using DarV2.Models;

namespace DarV2.Service
{
    public interface IStudentWarningService
    {
        Task<StudentWarningViewDTO> CreateAsync(StudentWarningCreateDTO dto, string? createdByUserId);
        Task<StudentWarningViewDTO?> GetByIdAsync(int id);
        Task<IEnumerable<StudentWarningViewDTO>> GetByStudentIdAsync(int studentId);
        Task<StudentWarningPagedResultDTO> GetAllAsync(int page, int pageSize, int? studentId, WarningType? warningType, int? groupId, DateTime? fromDate, DateTime? toDate, string? search);
        Task<StudentWarningSummaryDTO> GetSummaryAsync(int? studentId);
        Task<bool> UpdateAsync(int id, StudentWarningUpdateDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}