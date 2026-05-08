using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpPost("batch")]
        [Authorize]
        public async Task<IActionResult> SaveBatch([FromBody] AttendanceBatchDTO batch)
        {
            await _service.SaveBatchAsync(batch);
            return NoContent();
        }
    }
}
