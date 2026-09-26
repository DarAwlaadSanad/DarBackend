using DarV2.Models;
using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemorizationController : ControllerBase
    {
        private readonly IMemorizationService _service;

        public MemorizationController(IMemorizationService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ManageMemorization)]
        public async Task<IActionResult> Add([FromBody] MemorizationRecordCreateDTO dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(Add), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.ManageMemorization)]
        public async Task<IActionResult> Update(int id, [FromBody] MemorizationRecordCreateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ManageMemorization)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
