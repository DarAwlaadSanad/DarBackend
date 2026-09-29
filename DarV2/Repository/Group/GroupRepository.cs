using DarV2.Context;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Repository
{
    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {
        public GroupRepository(DarContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Group>> GetAllWithIncludes()
        {
            return await _db.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Room)
                .Include(g => g.StudentGroups)
                    .ThenInclude(sg => sg.Student)
                .ToListAsync();
        }

        public async Task<Group?> GetGroupWithDetailsAsync(int groupId)
        {
            return await _db.Groups
                .AsNoTracking()
                .Where(g => g.Id == groupId)
                .Include(g => g.Teacher)
                .Include(g => g.Room)
                .FirstOrDefaultAsync();
        }
    }
}
