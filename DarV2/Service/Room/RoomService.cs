using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;

namespace DarV2.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _uow;

        public RoomService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<RoomViewDTO>> GetAllAsync()
        {
            var rooms = await _uow.Rooms.GetAllAsync();
            return rooms.Select(MapToView);
        }

        public async Task<RoomViewDTO?> GetByIdAsync(int id)
        {
            var room = await _uow.Rooms.GetByIdAsync(id);
            return room == null ? null : MapToView(room);
        }

        public async Task<RoomViewDTO> AddAsync(CreateRoomDTO dto)
        {
            var room = new Room
            {
                Name = dto.Name,
                Notes = dto.Notes
            };

            await _uow.Rooms.AddAsync(room);
            await _uow.CommitAsync();

            return MapToView(room);
        }

        public async Task<RoomViewDTO> UpdateAsync(UpdateRoomDTO dto)
        {
            var room = await _uow.Rooms.GetByIdAsync(dto.Id);
            if (room == null) throw new Exception("الغرفة غير موجودة");

            room.Name = dto.Name;
            room.Notes = dto.Notes;

            _uow.Rooms.Update(room);
            await _uow.CommitAsync();

            return MapToView(room);
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var room = await _uow.Rooms.GetByIdAsync(id);
            if (room == null) return false;

            _uow.Rooms.Remove(room);
            await _uow.CommitAsync();
            return true;
        }

        private static RoomViewDTO MapToView(Room r)
        {
            return new RoomViewDTO
            {
                Id = r.Id,
                Name = r.Name,
                Notes = r.Notes
            };
        }
    }
}
