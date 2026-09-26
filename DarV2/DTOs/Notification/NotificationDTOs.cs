using System;
using System.Collections.Generic;

namespace DarV2.DTOs.Notification
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? SessionId { get; set; }
        public int? GroupId { get; set; }
    }

    public class NotificationListResponseDTO
    {
        public int UnreadCount { get; set; }
        public List<NotificationDTO> Notifications { get; set; } = new List<NotificationDTO>();
    }
}