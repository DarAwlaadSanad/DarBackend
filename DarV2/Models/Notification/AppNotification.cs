using System;

namespace DarV2.Models
{
    public class AppNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "General"; // SessionReminder, SubstituteAssignment, General

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}