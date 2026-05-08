using DarV2.DTOs;
using DarV2.Models;

namespace DarV2.Service
{
    public interface IGroupService
    {
        Task<IEnumerable<GroupCardDTO>> GetAllAsync();
        Task<GroupDetailsDTO?> GetByIdAsync(int groupId,int month,int year);
        Task<Group?> CreateAsync(GroupAddDTO dto);
        Task<bool> UpdateAsync(int id, GroupAddDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
