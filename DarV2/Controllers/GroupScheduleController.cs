using DarV2.Models;
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
            try
            {
                var created = await _service.AddAsync(dto);
                var resultDto = new GroupScheduleViewDTO
                {
                    Id = created.Id,
                    GroupId = created.GroupId,
                    DayOfWeek = (int)created.DayOfWeek,
                    StartTime = created.StartTime,
                    EndTime = created.EndTime,
                    EffectiveFrom = created.EffectiveFrom,
                    EffectiveTo = created.EffectiveTo,
                    IsActive = created.IsActive
                };
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateGroupScheduleDTO dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null) return NotFound();
                var resultDto = new GroupScheduleViewDTO
                {
                    Id = updated.Id,
                    GroupId = updated.GroupId,
                    DayOfWeek = (int)updated.DayOfWeek,
                    StartTime = updated.StartTime,
                    EndTime = updated.EndTime,
                    EffectiveFrom = updated.EffectiveFrom,
                    EffectiveTo = updated.EffectiveTo,
                    IsActive = updated.IsActive
                };
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
        {
            var items = await _service.GetByGroupAsync(groupId);
            return Ok(items);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyTimetable()
        {
            var items = await _service.GetAllActiveWeeklySchedulesAsync();
            return Ok(items);
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] string? teacherId = null)
        {
            try
            {
                var pdfBytes = await _service.ExportWeeklySchedulePdfAsync(teacherId);
                return File(pdfBytes, "application/pdf", "Weekly_Schedule.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
