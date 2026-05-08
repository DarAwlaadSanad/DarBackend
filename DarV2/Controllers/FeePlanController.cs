using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeePlanController : ControllerBase
    {
        private readonly IFeePlanService _service;

        public FeePlanController(IFeePlanService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] FeePlanAddDTO dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(Add), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int groupId)
        {
            var items = await _service.GetAllPlans(groupId);
            if(items == null || items.Count == 0) return NotFound("لا توجد خطط دفع");
            return Ok(items);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _service.DeactivateAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
