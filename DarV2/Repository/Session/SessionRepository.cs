using DarV2.Context;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DarV2.Repository
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        public SessionRepository(DarContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Session>> GetSessionsByGroupAndMonthAsync(int groupId, int month, int year)
        {
            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1);

            return await _db.Sessions
                .Where(s => s.GroupId == groupId && s.SessionDate >= start && s.SessionDate < end)
                .ToListAsync();
        }

        public async Task<bool> SessionExistsAsync(int groupId, DateOnly date, TimeSpan startTime)
        {
            return await _db.Sessions.AnyAsync(s =>
                s.GroupId == groupId &&
                s.SessionDate == date &&
                s.StartTime == startTime);
        }
    }
}
