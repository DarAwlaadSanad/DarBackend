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
    public class StudentFeeController : ControllerBase
    {
        private readonly IStudentFeeService _service;

        public StudentFeeController(IStudentFeeService service)
        {
            _service = service;
        }

        [HttpPost("generate/{feePlanId}")]
        [Authorize(Policy = Permissions.ManageGroupFees)]
        public async Task<IActionResult> Generate(int feePlanId, [FromQuery] int groupId, [FromQuery] int month, [FromQuery] int year)
        {
            await _service.GenerateForFeePlanAsync(feePlanId, groupId, month, year);
            return NoContent();
        }

        [HttpPut("{id}/payment")]
        [Authorize(Policy = Permissions.ManageGroupFees)]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdateStudentFeePaymentDTO dto)
        {
            var ok = await _service.UpdatePaymentAsync(id, dto.AmountPaid, dto.PaymentDate);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/exempt")]
        [Authorize(Policy = Permissions.ExemptFees)]
        public async Task<IActionResult> ExemptStudent(int id, [FromBody] ExemptStudentFeeDTO dto)
        {
            var ok = await _service.ExemptStudentAsync(id, dto.Reason);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/cancel-exempt")]
        [Authorize(Policy = Permissions.ExemptFees)]
        public async Task<IActionResult> CancelExemption(int id)
        {
            var ok = await _service.CancelExemptionAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int groupId, [FromQuery] int month, [FromQuery] int year)
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

                var items = await _service.GetAllAsync(groupId, month, year);
                var myItems = items.Where(f => f.StudentId == sId).ToList();
                return Ok(myItems);
            }

            var isAuthorized = User.IsInRole("Admin") ||
                               User.IsInRole("SuperAdmin") ||
                               User.HasClaim("Permission", Permissions.ViewGroupFees) ||
                               User.HasClaim("Permission", Permissions.ManageGroupFees);

            if (!isAuthorized)
            {
                return Forbid();
            }

            var allItems = await _service.GetAllAsync(groupId, month, year);
            return Ok(allItems);
        }

        [HttpGet("all")]
        [Authorize(Policy = Permissions.ViewFees)]
        public async Task<IActionResult> GetAllWithoutFilter(int month, int year)
        {
            var items = await _service.GetAllWithoutFilterAsync(month, year);
            return Ok(items);
        }

        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value ??
                                     User.Claims.FirstOrDefault(c => c.Type.Equals("studentId", StringComparison.OrdinalIgnoreCase))?.Value;

                if (!int.TryParse(studentIdClaim, out var sId) || sId != studentId)
                {
                    return Forbid();
                }
            }

            var items = await _service.GetByStudentIdAsync(studentId);
            return Ok(items);
        }
    }
}