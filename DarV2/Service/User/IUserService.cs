using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IUserService
    {
        Task<IEnumerable<UserViewDTO>> GetAllAsync();
        Task<IEnumerable<UserViewDTO>> GetTeachersAsync();
    }
}
