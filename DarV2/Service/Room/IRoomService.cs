using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomViewDTO>> GetAllAsync();
        Task<RoomViewDTO?> GetByIdAsync(int id);
        Task<RoomViewDTO> AddAsync(CreateRoomDTO dto);
        Task<RoomViewDTO> UpdateAsync(UpdateRoomDTO dto);
        Task<bool> RemoveAsync(int id);
    }
}
