using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DarV2.DTOs;
using DarV2.Models;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentWarningController : ControllerBase
    {
        private readonly IStudentWarningService _warningService;

        public StudentWarningController(IStudentWarningService warningService)
        {
            _warningService = warningService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentWarningCreateDTO dto)
        {
            if (dto.StudentId <= 0)
                return BadRequest("ÙŠØ¬Ø¨ ØªØ­Ø¯ÙŠØ¯ Ø§Ù„Ø·Ø§Ù„Ø¨");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var result = await _warningService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ø¥Ù†Ø°Ø§Ø±: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? studentId = null,
            [FromQuery] WarningType? warningType = null,
            [FromQuery] int? groupId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? search = null)
        {
            // If student role, restrict to their own warnings
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (int.TryParse(studentIdClaim, out var sId))
                {
                    studentId = sId;
                }
                else
                {
                    return Forbid();
                }
            }

            var result = await _warningService.GetAllAsync(page, pageSize, studentId, warningType, groupId, fromDate, toDate, search);
            return Ok(result);
        }

        [HttpGet("my-warnings")]
        public async Task<IActionResult> GetMyWarnings()
        {
            var studentIdClaim = User.FindFirst("studentId")?.Value ?? 
                                 User.Claims.FirstOrDefault(c => c.Type.Equals("studentId", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(studentIdClaim) || !int.TryParse(studentIdClaim, out var sId))
            {
                return BadRequest("لم يتم العثور على معرف الطالب");
            }

            var items = await _warningService.GetByStudentIdAsync(sId);
            return Ok(items);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value ?? 
                                     User.Claims.FirstOrDefault(c => c.Type.Equals("studentId", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrEmpty(studentIdClaim) && studentIdClaim != studentId.ToString())
                {
                    return Forbid();
                }
            }

            var items = await _warningService.GetByStudentIdAsync(studentId);
            return Ok(items);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? studentId = null)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (int.TryParse(studentIdClaim, out var sId))
                {
                    studentId = sId;
                }
            }

            var summary = await _warningService.GetSummaryAsync(studentId);
            return Ok(summary);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var warning = await _warningService.GetByIdAsync(id);
            if (warning == null) return NotFound("Ø§Ù„Ø¥Ù†Ø°Ø§Ø± ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯");

            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (studentIdClaim != warning.StudentId.ToString())
                {
                    return Forbid();
                }
            }

            return Ok(warning);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentWarningUpdateDTO dto)
        {
            var ok = await _warningService.UpdateAsync(id, dto);
            if (!ok) return NotFound("Ø§Ù„Ø¥Ù†Ø°Ø§Ø± ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _warningService.DeleteAsync(id);
            if (!ok) return NotFound("Ø§Ù„Ø¥Ù†Ø°Ø§Ø± ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯");
            return NoContent();
        }
    }
}