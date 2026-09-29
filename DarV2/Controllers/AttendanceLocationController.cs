using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarV2.DTOs.AttendanceLocation;
using DarV2.Service.AttendanceLocation;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceLocationController : ControllerBase
    {
        private readonly IAttendanceLocationService _service;

        public AttendanceLocationController(IAttendanceLocationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _service.GetActiveLocationsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "الموقع غير موجود" });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceLocationDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceLocationDTO dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "معرف الموقع غير متطابق" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.UpdateAsync(dto);
            if (result == null) return NotFound(new { message = "الموقع غير موجود" });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = "الموقع غير موجود" });

            return Ok(new { message = "تم حذف الموقع بنجاح" });
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _service.ToggleActiveAsync(id);
            if (!result) return NotFound(new { message = "الموقع غير موجود" });

            return Ok(new { message = "تم تعديل حالة الموقع بنجاح" });
        }
    }
}
