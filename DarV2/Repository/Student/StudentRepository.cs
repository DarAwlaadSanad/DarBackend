using DarV2.Context;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Repository
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(DarContext db) : base(db)
        {
        }

        public async Task<List<Student>> GetStudentsGroupAsync(int groupId)
        {
            return await _db.Students.Include(s=>s.StudentGroups)
                .Where(s => s.StudentGroups.Any(sg => sg.GroupId == groupId))
                .ToListAsync();
        }
        public async Task<Student> GetStudentAsync(int id)
        {
            return await _db.Students.Include(s=>s.AcademicYear).Include(s=>s.StudentGroups).ThenInclude(sg=>sg.Group).Include(s=>s.MemorizationRecords).Include(s => s.Images).Include(s => s.Phones).FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Student> GetStudentPage(int id)
        {
            return await _db.Students.Include(s => s.StudentGroups).ThenInclude(sd=>sd.Group).ThenInclude(g=>g.Teacher).Include(s => s.Evaluations).Include(s => s.Attendances).FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<List<Student>> GetAllStudents()
        {
            return await _db.Students.Include(s => s.StudentGroups).ThenInclude(sd => sd.Group).ThenInclude(g => g.Teacher).Include(s=>s.AcademicYear).ToListAsync();
        }
    }
}
