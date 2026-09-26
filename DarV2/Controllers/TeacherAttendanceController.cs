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
    [Authorize] // Require login
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly ITeacherAttendanceService _service;

        public TeacherAttendanceController(ITeacherAttendanceService service)
        {
            _service = service;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

                [HttpGet("status")]
        public async Task<IActionResult> GetTodayAttendanceStatus()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var status = await _service.GetTodayAttendanceStatusAsync(userId);
            return Ok(status);
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayRecord()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var record = await _service.GetTodayRecordAsync(userId);
            if (record == null) return NoContent();

            return Ok(record);
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _service.CheckInAsync(userId);
            return Ok(response);
        }

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _service.CheckOutAsync(userId);
            return Ok(response);
        }

        [HttpPost("mark-absent")]
        [Authorize]
        public async Task<IActionResult> MarkAbsent([FromBody] MarkTeacherAbsentDTO dto)
        {
            if (!User.IsInRole("Admin") && 
                !User.IsInRole("SuperAdmin") && 
                !User.IsInRole("مشرف") && 
                !User.IsInRole("Supervisor") && 
                !User.HasClaim("Permission", Permissions.ManageTeacherAttendance))
            {
                return Forbid();
            }

            var success = await _service.MarkTeacherAbsentAsync(dto);
            if (!success) return BadRequest();
            return Ok();
        }

        
        [HttpPost("cancel-absent")]
        [Authorize]
        public async Task<IActionResult> CancelAbsent([FromBody] MarkTeacherAbsentDTO dto)
        {
            if (!User.IsInRole("Admin") && 
                !User.IsInRole("SuperAdmin") && 
                !User.IsInRole("مشرف") && 
                !User.IsInRole("Supervisor") && 
                !User.HasClaim("Permission", Permissions.ManageTeacherAttendance))
            {
                return Forbid();
            }

            var success = await _service.CancelTeacherAbsentAsync(dto);
            if (!success) return BadRequest();
            return Ok();
        }

[HttpGet("monthly-report")]
        [Authorize]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var report = await _service.GetMonthlyReportAsync(year, month);
            return Ok(report);
        }
    }
}
