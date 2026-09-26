using DarV2.DTOs.Notification;
using DarV2.Models;
using System.Threading.Tasks;

namespace DarV2.Service.Notification
{
    public interface INotificationService
    {
        Task<NotificationListResponseDTO> GetUserNotificationsAsync(string userId, int count = 30);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<AppNotification> CreateNotificationAsync(string userId, string title, string message, string type, int? sessionId = null, int? groupId = null);
    }
}