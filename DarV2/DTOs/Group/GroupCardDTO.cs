namespace DarV2.DTOs
{
    public class GroupCardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int StudentCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public bool IsOnline { get; set; }
        public int? RoomId { get; set; }
        public string? RoomName { get; set; }
    }
}
