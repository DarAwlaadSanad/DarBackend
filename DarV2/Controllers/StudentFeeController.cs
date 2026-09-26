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
            await _service.GenerateForFeePlanAsync(feePlanId,groupId, month, year);
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
        [Authorize(Policy = Permissions.ViewGroupFees)]
        public async Task<IActionResult> GetAll([FromQuery] int groupId , [FromQuery] int month, [FromQuery] int year)
        {
            var items = await _service.GetAllAsync(groupId, month, year);
            return Ok(items);
        }

        [HttpGet("all")]
        [Authorize(Policy = Permissions.ViewFees)]
        public async Task<IActionResult> GetAllWithoutFilter(int month,int year)
        {
            var items = await _service.GetAllWithoutFilterAsync(month, year);
            return Ok(items);
        }
    }
}
