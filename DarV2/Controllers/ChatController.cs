using DarV2.DTOs;
using DarV2.Hubs;
using DarV2.Models;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("sub")?.Value;

        // GET api/Chat/staff-room
        [HttpGet("staff-room")]
        public async Task<IActionResult> GetStaffRoom()
            => Ok(await _chatService.GetStaffRoomAsync());

        // GET api/Chat/student-rooms  (requires ViewStudentChats)
        [HttpGet("student-rooms")]
        [Authorize(Policy = Permissions.ViewStudentChats)]
        public async Task<IActionResult> GetStudentRooms()
            => Ok(await _chatService.GetStudentRoomsAsync());

        // GET api/Chat/student-room/{studentId}  (admin opens student room)
        [HttpGet("student-room/{studentId}")]
        [Authorize(Policy = Permissions.ViewStudentChats)]
        public async Task<IActionResult> GetStudentRoom(int studentId)
        {
            var room = await _chatService.GetOrCreateStudentRoomAsync(studentId);
            return room == null ? NotFound() : Ok(room);
        }

        // GET api/Chat/my-room  (student views their own room)
        [HttpGet("my-room")]
        public async Task<IActionResult> GetMyRoom()
        {
            var studentIdClaim = User.FindFirstValue("StudentId") ?? User.FindFirstValue("studentId");
            if (int.TryParse(studentIdClaim, out int sid))
            {
                var room = await _chatService.GetOrCreateStudentRoomAsync(sid);
                return room == null ? NotFound() : Ok(room);
            }
            return Forbid();
        }

        // GET api/Chat/{roomId}/messages
        [HttpGet("{roomId}/messages")]
        public async Task<IActionResult> GetMessages(int roomId, [FromQuery] int page = 1)
            => Ok(await _chatService.GetMessagesAsync(roomId, page));

        // POST api/Chat/{roomId}/send  (staff sends to any room)
        [HttpPost("{roomId}/send")]
        public async Task<IActionResult> Send(int roomId, [FromBody] SendMessageDTO dto)
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();
            var msg = await _chatService.SendMessageAsUserAsync(roomId, uid, dto.Content);
            if (msg == null) return NotFound();
            await _hubContext.Clients.Group($"room_{roomId}").SendAsync("ReceiveMessage", msg);
            return Ok(msg);
        }

        // POST api/Chat/my-room/send  (student sends to own room)
        [HttpPost("my-room/send")]
        public async Task<IActionResult> StudentSend([FromBody] SendMessageDTO dto)
        {
            var studentIdClaim = User.FindFirstValue("StudentId") ?? User.FindFirstValue("studentId");
            if (!int.TryParse(studentIdClaim, out int sid)) return Forbid();
            var room = await _chatService.GetOrCreateStudentRoomAsync(sid);
            if (room == null) return NotFound();
            var msg = await _chatService.SendMessageAsStudentAsync(room.Id, sid, dto.Content);
            if (msg == null) return BadRequest();
            await _hubContext.Clients.Group($"room_{room.Id}").SendAsync("ReceiveMessage", msg);
            return Ok(msg);
        }

        // PUT api/Chat/{roomId}/read
        [HttpPut("{roomId}/read")]
        public async Task<IActionResult> MarkRead(int roomId)
        {
            var uid = UserId;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();
            await _chatService.MarkRoomAsReadAsync(roomId, uid);
            return NoContent();
        }
    }
}
