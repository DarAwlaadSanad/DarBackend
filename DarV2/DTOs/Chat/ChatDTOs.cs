namespace DarV2.DTOs
{
    public class ChatMessageDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string SenderName { get; set; }
        public string? SenderId { get; set; }
        public int? StudentSenderId { get; set; }
        public bool IsStudent { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ChatRoomDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Staff" | "StudentSupport"
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public ChatMessageDTO? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class SendMessageDTO
    {
        public string Content { get; set; }
    }
}
