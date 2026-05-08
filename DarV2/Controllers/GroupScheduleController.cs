using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupScheduleController : ControllerBase
    {
        private readonly IGroupScheduleService _service;

        public GroupScheduleController(IGroupScheduleService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateGroupScheduleDTO dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetByGroup), new { groupId = created.GroupId }, created);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
        {
            var items = await _service.GetByGroupAsync(groupId);
            return Ok(items);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Remove(int id)
        {
            var ok = await _service.RemoveAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
