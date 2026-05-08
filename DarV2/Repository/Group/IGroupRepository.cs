using DarV2.Models;
namespace DarV2.Repository
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        public Task<IEnumerable<Group>> GetAllWithIncludes();
        public Task<Group?> GetGroupWithDetailsAsync(int groupId);
    }
}
