using DarV2.Context;
using DarV2.DTOs;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class ChatService : IChatService
    {
        private readonly DarContext _db;
        public ChatService(DarContext db) => _db = db;

        // ─── Helpers ────────────────────────────────────────────────────
        private static ChatMessageDTO ToDTO(ChatMessage m) => new()
        {
            Id = m.Id,
            RoomId = m.RoomId,
            SenderId = m.SenderId,
            StudentSenderId = m.StudentSenderId,
            IsStudent = m.StudentSenderId.HasValue,
            SenderName = m.StudentSenderId.HasValue
                ? (m.StudentSender?.FullName ?? "طالب")
                : (m.Sender?.UserName ?? "مستخدم"),
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead
        };

        private static ChatRoomDTO ToRoomDTO(ChatRoom r, ChatMessage? last, int unread) => new()
        {
            Id = r.Id,
            Name = r.Type == ChatRoomType.Staff ? r.Name : (r.Student?.FullName ?? r.Name),
            Type = r.Type == ChatRoomType.Staff ? "Staff" : "StudentSupport",
            StudentId = r.StudentId,
            StudentName = r.Student?.FullName,
            LastMessage = last == null ? null : ToDTO(last),
            UnreadCount = unread
        };

        // ─── Staff global room ───────────────────────────────────────────
        public async Task<ChatRoomDTO?> GetStaffRoomAsync()
        {
            var room = await _db.ChatRooms
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Type == ChatRoomType.Staff);

            if (room == null)
            {
                room = new ChatRoom { Type = ChatRoomType.Staff, Name = "غرفة الموظفين" };
                _db.ChatRooms.Add(room);
                await _db.SaveChangesAsync();
            }

            var last = await _db.ChatMessages.Where(m => m.RoomId == room.Id)
                .OrderByDescending(m => m.SentAt).FirstOrDefaultAsync();
            return ToRoomDTO(room, last, 0);
        }

        // ─── All student rooms (for admin/viewer) ───────────────────────
        public async Task<IEnumerable<ChatRoomDTO>> GetStudentRoomsAsync()
        {
            var rooms = await _db.ChatRooms
                .Include(r => r.Student)
                .Where(r => r.Type == ChatRoomType.StudentSupport)
                .OrderBy(r => r.Student!.FullName)
                .ToListAsync();

            var result = new List<ChatRoomDTO>();
            foreach (var r in rooms)
            {
                var last = await _db.ChatMessages.Where(m => m.RoomId == r.Id)
                    .OrderByDescending(m => m.SentAt).FirstOrDefaultAsync();
                var unread = await _db.ChatMessages.CountAsync(m => m.RoomId == r.Id && !m.IsRead && m.StudentSenderId.HasValue);
                result.Add(ToRoomDTO(r, last, unread));
            }
            return result;
        }

        // ─── Get/create student support room ────────────────────────────
        public async Task<ChatRoomDTO?> GetOrCreateStudentRoomAsync(int studentId)
        {
            var student = await _db.Students.FindAsync(studentId);
            if (student == null) return null;

            var room = await _db.ChatRooms.Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Type == ChatRoomType.StudentSupport && r.StudentId == studentId);

            if (room == null)
            {
                room = new ChatRoom
                {
                    Type = ChatRoomType.StudentSupport,
                    Name = student.FullName,
                    StudentId = studentId,
                    Student = student
                };
                _db.ChatRooms.Add(room);
                await _db.SaveChangesAsync();
            }

            var last = await _db.ChatMessages.Where(m => m.RoomId == room.Id)
                .OrderByDescending(m => m.SentAt).FirstOrDefaultAsync();
            var unread = await _db.ChatMessages.CountAsync(m => m.RoomId == room.Id && !m.IsRead && m.StudentSenderId.HasValue);
            return ToRoomDTO(room, last, unread);
        }

        // ─── Get rooms for a staff user ──────────────────────────────────
        public async Task<IEnumerable<ChatRoomDTO>> GetRoomsForUserAsync(string userId)
        {
            var list = new List<ChatRoomDTO>();
            var staffRoom = await GetStaffRoomAsync();
            if (staffRoom != null) list.Add(staffRoom);
            list.AddRange(await GetStudentRoomsAsync());
            return list;
        }

        // ─── Messages ────────────────────────────────────────────────────
        public async Task<IEnumerable<ChatMessageDTO>> GetMessagesAsync(int roomId, int page = 1, int pageSize = 50)
        {
            return await _db.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.StudentSender)
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SentAt)
                .Select(m => ToDTO(m))
                .ToListAsync();
        }

        public async Task<ChatMessageDTO?> SendMessageAsUserAsync(int roomId, string userId, string content)
        {
            var room = await _db.ChatRooms.FindAsync(roomId);
            if (room == null) return null;

            var msg = new ChatMessage
            {
                RoomId = roomId,
                SenderId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            return await _db.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.StudentSender)
                .Where(m => m.Id == msg.Id)
                .Select(m => ToDTO(m))
                .FirstAsync();
        }

        public async Task<ChatMessageDTO?> SendMessageAsStudentAsync(int roomId, int studentId, string content)
        {
            var room = await _db.ChatRooms.FindAsync(roomId);
            if (room == null || room.StudentId != studentId) return null;

            var msg = new ChatMessage
            {
                RoomId = roomId,
                StudentSenderId = studentId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            return await _db.ChatMessages
                .Include(m => m.StudentSender)
                .Where(m => m.Id == msg.Id)
                .Select(m => ToDTO(m))
                .FirstAsync();
        }

        public async Task MarkRoomAsReadAsync(int roomId, string userId)
        {
            var msgs = _db.ChatMessages.Where(m => m.RoomId == roomId && !m.IsRead && m.StudentSenderId.HasValue);
            await msgs.ForEachAsync(m => m.IsRead = true);
            await _db.SaveChangesAsync();
        }
    }
}

