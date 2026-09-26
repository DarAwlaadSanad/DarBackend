using DarV2.Models;
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
        [Authorize(Policy = Permissions.ManageFeePlans)]
        public async Task<IActionResult> Add([FromBody] FeePlanAddDTO dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(Add), new { id = created.Id }, created);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewFeePlans)]
        public async Task<IActionResult> GetAll(int groupId)
        {
            var items = await _service.GetAllPlans(groupId);
            if(items == null) return Ok(new List<object>()); // return empty list
            return Ok(items);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ManageFeePlans)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _service.DeactivateAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
