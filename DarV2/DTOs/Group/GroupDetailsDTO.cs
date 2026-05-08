namespace DarV2.DTOs
{
    public class GroupDetailsDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string? Description { get; set; }
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // Student with data needed for attendance and evaluation
        public List<SessionViewDTO> Sessions { get; set; } = new List<SessionViewDTO>();
        public List<StudentInGroupDTO> Students { get; set; } = new List<StudentInGroupDTO>();

    }
}
