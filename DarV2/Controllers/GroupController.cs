using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _groupService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> Get(int groupId,int month,int year)
        {
            var item = await _groupService.GetByIdAsync(groupId,month,year);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] GroupAddDTO dto)
        {
            var created = await _groupService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] GroupAddDTO dto)
        {
            var ok = await _groupService.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _groupService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
