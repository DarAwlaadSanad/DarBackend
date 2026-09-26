using DarV2.Models;
using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _service;

        public ExamController(IExamService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExamDTO dto)
        {
            var res = await _service.CreateExamAsync(dto);
            return Ok(res);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetByGroup(int groupId)
        {
            var res = await _service.GetExamsByGroupAsync(groupId);
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetExamByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteExamAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            var res = await _service.GetExamResultsAsync(id);
            return Ok(res);
        }

        [HttpPut("{id}/results")]
        public async Task<IActionResult> SaveResults(int id, [FromBody] SaveExamResultsDTO dto)
        {
            var ok = await _service.SaveExamResultsAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentResults(int studentId)
        {
            var res = await _service.GetStudentExamResultsAsync(studentId);
            return Ok(res);
        }
    }
}
