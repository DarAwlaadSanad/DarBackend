namespace DarV2.DTOs
{
    public class GroupAddDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? TeacherId { get; set; }
        public bool IsOnline { get; set; }
        public int? RoomId { get; set; }
    }
}
