namespace DarV2.Models
{
    public enum ChatRoomType
    {
        Staff = 1,
        StudentSupport = 2
    }

    public class ChatRoom
    {
        public int Id { get; set; }
        public ChatRoomType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public ChatRoom Room { get; set; }

        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }

        public int? StudentSenderId { get; set; }
        public Student? StudentSender { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
