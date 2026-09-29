using System.Collections.Generic;
using System.Threading.Tasks;
using DarV2.DTOs.AttendanceLocation;

namespace DarV2.Service.AttendanceLocation
{
    public interface IAttendanceLocationService
    {
        Task<List<AttendanceLocationDTO>> GetAllAsync();
        Task<List<AttendanceLocationDTO>> GetActiveLocationsAsync();
        Task<AttendanceLocationDTO?> GetByIdAsync(int id);
        Task<AttendanceLocationDTO> CreateAsync(CreateAttendanceLocationDTO dto);
        Task<AttendanceLocationDTO?> UpdateAsync(UpdateAttendanceLocationDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }
}
