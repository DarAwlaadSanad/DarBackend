using DarV2.Models;

namespace DarV2.Repository
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
            Task<IEnumerable<Session>> GetSessionsByGroupAndMonthAsync(int groupId,int month,int year);
        Task<bool> SessionExistsAsync(int groupId, DateOnly date, TimeSpan startTime);
    }
}
