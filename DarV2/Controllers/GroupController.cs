using System.Security.Claims;
using DarV2.Models;
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
        [Authorize(Policy = Permissions.ViewGroups)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _groupService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{groupId}")]
        [Authorize]
        public async Task<IActionResult> Get(int groupId, [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            if (month <= 0) month = DateTime.Now.Month;
            if (year <= 0) year = DateTime.Now.Year;

            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value ??
                                     User.Claims.FirstOrDefault(c => c.Type.Equals("studentId", StringComparison.OrdinalIgnoreCase))?.Value;

                if (!int.TryParse(studentIdClaim, out var sId))
                {
                    return Forbid();
                }

                var item = await _groupService.GetByIdAsync(groupId, month, year);
                if (item == null) return NotFound();

                // Check if student belongs to this group
                if (!item.Students.Any(s => s.StudentId == sId))
                {
                    return Forbid();
                }

                // Privacy: Clear other students' detailed records and totals
                foreach (var st in item.Students)
                {
                    if (st.StudentId != sId)
                    {
                        st.Records = new Dictionary<int, SessionRecordDTO>();
                        st.TotalPresent = 0;
                        st.TotalEvaluation = 0;
                    }
                }

                return Ok(item);
            }

            var isAuthorized = User.IsInRole("Admin") ||
                               User.IsInRole("SuperAdmin") ||
                               User.HasClaim("Permission", Permissions.ViewGroups) ||
                               User.HasClaim("Permission", Permissions.ManageGroups);

            if (!isAuthorized)
            {
                return Forbid();
            }

            var result = await _groupService.GetByIdAsync(groupId, month, year);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ManageGroups)]
        public async Task<IActionResult> Create([FromBody] GroupAddDTO dto)
        {
            var created = await _groupService.CreateAsync(dto);
            if (created == null) return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.ManageGroups)]
        public async Task<IActionResult> Update(int id, [FromBody] GroupAddDTO dto)
        {
            var ok = await _groupService.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DeleteGroups)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _groupService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
