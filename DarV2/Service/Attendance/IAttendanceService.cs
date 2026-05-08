using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IAttendanceService
    {
        Task SaveBatchAsync(AttendanceBatchDTO batch);
    }
}
