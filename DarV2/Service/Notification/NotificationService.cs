using DarV2.Context;
using DarV2.DTOs.Notification;
using DarV2.Hubs;
using DarV2.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly DarContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(DarContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<NotificationListResponseDTO> GetUserNotificationsAsync(string userId, int count = 30)
        {
            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            var list = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Select(n => new NotificationDTO
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    SessionId = n.SessionId,
                    GroupId = n.GroupId
                })
                .ToListAsync();

            return new NotificationListResponseDTO
            {
                UnreadCount = unreadCount,
                Notifications = list
            };
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var item = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (item == null) return false;

            item.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppNotification> CreateNotificationAsync(string userId, string title, string message, string type, int? sessionId = null, int? groupId = null)
        {
            var notification = new AppNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                SessionId = sessionId,
                GroupId = groupId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var dto = new NotificationDTO
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                SessionId = notification.SessionId,
                GroupId = notification.GroupId
            };

            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", dto);
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", dto);
            }
            catch
            {
            }

            return notification;
        }
    }
}
