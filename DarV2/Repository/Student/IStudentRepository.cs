using DarV2.Models;

namespace DarV2.Repository
{
    public interface IStudentRepository: IGenericRepository<Student>
    {
        Task<List<Student>> GetStudentsGroupAsync(int groupId);
        Task<Student> GetStudentAsync(int id);
        Task<Student> GetStudentPage(int id);
        Task<List<Student>> GetAllStudents();


    }
}
