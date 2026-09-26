using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IChatService
    {
        // Rooms
        Task<IEnumerable<ChatRoomDTO>> GetRoomsForUserAsync(string userId);
        Task<IEnumerable<ChatRoomDTO>> GetStudentRoomsAsync(); // All student support rooms (for admins)
        Task<ChatRoomDTO?> GetOrCreateStudentRoomAsync(int studentId); // get/create a room for a student
        Task<ChatRoomDTO?> GetStaffRoomAsync(); // The one global staff room

        // Messages
        Task<IEnumerable<ChatMessageDTO>> GetMessagesAsync(int roomId, int page = 1, int pageSize = 50);
        Task<ChatMessageDTO?> SendMessageAsUserAsync(int roomId, string userId, string content);
        Task<ChatMessageDTO?> SendMessageAsStudentAsync(int roomId, int studentId, string content);
        Task MarkRoomAsReadAsync(int roomId, string userId);
    }
}
