using DarV2.Models;
using DarV2.DTOs.TeacherAttendance;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _service;

        public SessionController(ISessionService service)
        {
            _service = service;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet("today-sessions")]
        [Authorize]
        public async Task<IActionResult> GetTodaySessions()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin") || 
                          User.IsInRole("SuperAdmin") || 
                          User.IsInRole("مشرف") || 
                          User.IsInRole("Supervisor") || 
                          User.HasClaim("Permission", Permissions.ManageAttendance) ||
                          User.HasClaim("Permission", Permissions.ViewAttendance);
            var isTeacher = User.IsInRole("Teacher");

            var sessions = await _service.GetTodaySessionsAsync(userId, isAdmin, isTeacher);
            return Ok(sessions);
        }

        [HttpGet("teacher-sessions")]
        [Authorize]
        public async Task<IActionResult> GetTeacherSessions([FromQuery] string teacherId, [FromQuery] string date)
        {
            if (!User.IsInRole("Admin") && 
                !User.IsInRole("SuperAdmin") && 
                !User.IsInRole("مشرف") && 
                !User.IsInRole("Supervisor") && 
                !User.HasClaim("Permission", Permissions.ViewTeacherAttendance) &&
                !User.HasClaim("Permission", Permissions.ManageTeacherAttendance) &&
                !User.HasClaim("Permission", Permissions.ManageSessions))
            {
                return Forbid();
            }

            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            var sessions = await _service.GetTeacherSessionsByDateAsync(teacherId, parsedDate);
            return Ok(sessions);
        }

        [HttpPost("assign-substitute")]
        [Authorize]
        public async Task<IActionResult> AssignSubstitute([FromBody] AssignSubstituteDTO dto)
        {
            if (!User.IsInRole("Admin") && 
                !User.IsInRole("SuperAdmin") && 
                !User.IsInRole("مشرف") && 
                !User.IsInRole("Supervisor") && 
                !User.HasClaim("Permission", Permissions.ManageTeacherAttendance) &&
                !User.HasClaim("Permission", Permissions.ManageSessions))
            {
                return Forbid();
            }

            try
            {
                var result = await _service.AssignSubstituteAsync(dto);
                if (!result) return NotFound(new { message = "لم يتم العثور على الحصة." });
                return Ok(new { message = "تم تعيين المعلم البديل بنجاح." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "حدث خطأ أثناء تعيين المعلم البديل: " + ex.Message });
            }
        }
    
        [HttpPost("revert-substitute/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> RevertSubstitute(int sessionId)
        {
            if (!User.IsInRole("Admin") && 
                !User.IsInRole("SuperAdmin") && 
                !User.IsInRole("مشرف") && 
                !User.IsInRole("Supervisor") && 
                !User.HasClaim("Permission", Permissions.ManageTeacherAttendance) &&
                !User.HasClaim("Permission", Permissions.ManageSessions))
            {
                return Forbid();
            }

            try
            {
                var result = await _service.RevertSubstituteAsync(sessionId);
                if (!result) return NotFound(new { message = "لم يتم العثور على الحصة." });
                return Ok(new { message = "تم إلغاء تعيين المعلم البديل بنجاح." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "حدث خطأ أثناء إلغاء تعيين المعلم البديل: " + ex.Message });
            }
        }

    }
}
