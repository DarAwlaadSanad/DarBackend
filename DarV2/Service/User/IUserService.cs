using DarV2.DTOs;
using DarV2.DTOs.AppUser;
using Microsoft.AspNetCore.Http;

namespace DarV2.Service
{
    public interface IUserService
    {
        Task<IEnumerable<UserViewDTO>> GetAllAsync();
        Task<IEnumerable<UserViewDTO>> GetTeachersAsync();
        Task<bool> AssignRolesAsync(string userId, List<string> roles);
        Task<UserProfileDTO?> GetProfileAsync(string userId);
        Task<(bool success, string? error, UserProfileDTO? profile)> UpdateProfileAsync(string userId, UpdateProfileDTO dto);
        Task<(bool success, string? error, string? photoUrl)> UpdateProfilePhotoAsync(string userId, IFormFile file);
        Task<bool> RemoveProfilePhotoAsync(string userId);
        Task<(bool success, string? error)> ChangePasswordAsync(string userId, ChangePasswordDTO dto);
        Task<(bool success, string? error, bool? newStatus)> ToggleStatusAsync(string currentUserId, string targetUserId);
        Task<(bool success, string? error)> DeleteUserAsync(string currentUserId, string targetUserId);
    }
}
