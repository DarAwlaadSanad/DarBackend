using DarV2.DTOs;
using Microsoft.AspNetCore.Http;

namespace DarV2.Service
{
    public interface IStudentService
    {
        Task<StudentPagedResultDTO> GetAllAsync(int page = 1, int pageSize = 20, int? academicYearId = null, int? groupId = null, string? search = null, bool? isActive = null);
        Task<StudentDetailsDTO?> GetByIdAsync(int id);
        Task<StudentDetailsDTO> CreateAsync(StudentAddDTO dto);
        Task<bool> UpdateAsync(int id, StudentUpdateDTO dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> AddImageAsync(int studentId, List<IFormFile> files);
        Task<bool> RemoveImageAsync(int imageId);

        Task<bool> CreatePhoneAsync(int studentId, string number);
        Task<bool> UpdatePhoneAsync(int phoneId, string number);
        Task<bool> DeletePhoneAsync(int phoneId);

        Task<bool> AssigenGroupToStudent(int studentId, int groupId);
        Task<bool> UnAssigenGroupToStudent(int studentId, int groupId);
        Task<bool> ValidSSNAsync(string ssn);

        Task<StudentLoginResponse> LoginAsync(string Code, string Password);
        Task ChangePasswordAsync(int studentId, string currentPassword, string newPassword);

        Task<IEnumerable<GroupCardDTO>> GetGroupsAsync(int studentId);

        
    }
}
