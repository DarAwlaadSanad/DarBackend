using DarV2.DTOs.Role;

namespace DarV2.Service
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDTO>> GetAllAsync();
        Task<RoleDTO?> GetByIdAsync(string id);
        Task<bool> CreateAsync(RoleAddDTO dto);
        Task<bool> UpdateAsync(string id, RoleAddDTO dto);
        Task<bool> DeleteAsync(string id);
    }
}
