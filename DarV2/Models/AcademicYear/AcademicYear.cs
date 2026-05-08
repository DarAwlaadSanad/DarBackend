namespace DarV2.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TypeSchool TypeSchool { get; set; } = TypeSchool.Public;
        public ICollection<Student> Students { get; set; }
    }
}
