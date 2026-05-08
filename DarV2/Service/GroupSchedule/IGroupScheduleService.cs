using DarV2.DTOs;
using DarV2.Models;

namespace DarV2.Service
{
    public interface IGroupScheduleService
    {
        Task<GroupSchedule> AddAsync(CreateGroupScheduleDTO dto);
        Task<IEnumerable<GroupScheduleViewDTO>> GetByGroupAsync(int groupId);
        Task<bool> RemoveAsync(int scheduleId);
    }
}
