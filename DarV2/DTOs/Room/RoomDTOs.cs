namespace DarV2.DTOs
{
    public class RoomViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateRoomDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class UpdateRoomDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
