using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DarV2.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            var studentIdClaim = Context.User?.FindFirstValue("studentId") ?? Context.User?.FindFirstValue("StudentId");
            if (int.TryParse(studentIdClaim, out int studentId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"student_{studentId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }

        public async Task SendMessage(int roomId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var studentIdClaim = Context.User?.FindFirstValue("studentId") ?? Context.User?.FindFirstValue("StudentId");
            if (int.TryParse(studentIdClaim, out int studentId))
            {
                var studentMsg = await _chatService.SendMessageAsStudentAsync(roomId, studentId, content);
                if (studentMsg != null)
                {
                    await Clients.Group($"room_{roomId}").SendAsync("ReceiveMessage", studentMsg);
                }
                return;
            }

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            var userMsg = await _chatService.SendMessageAsUserAsync(roomId, userId, content);
            if (userMsg != null)
            {
                await Clients.Group($"room_{roomId}").SendAsync("ReceiveMessage", userMsg);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
