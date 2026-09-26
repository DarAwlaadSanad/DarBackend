using DarV2.DTOs;
using DarV2.Models;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewRooms)]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _roomService.GetAllAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.ViewRooms)]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ManageRooms)]
        public async Task<IActionResult> Add([FromBody] CreateRoomDTO dto)
        {
            var room = await _roomService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.ManageRooms)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDTO dto)
        {
            if (id != dto.Id) return BadRequest();
            try
            {
                var room = await _roomService.UpdateAsync(dto);
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ManageRooms)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _roomService.RemoveAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
