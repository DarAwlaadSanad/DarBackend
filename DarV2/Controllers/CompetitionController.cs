using DarV2.Models;
using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionService _service;

        public CompetitionController(ICompetitionService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> Create([FromBody] CreateCompetitionDTO dto)
        {
            var res = await _service.CreateCompetitionAsync(dto);
            return Ok(res);
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewCompetitions)]
        public async Task<IActionResult> GetAll()
        {
            var res = await _service.GetAllCompetitionsAsync();
            return Ok(res);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.ViewCompetitions)]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetCompetitionByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteCompetitionAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("level")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> CreateLevel([FromBody] CreateCompetitionLevelDTO dto)
        {
            var res = await _service.CreateLevelAsync(dto);
            return Ok(res);
        }

        [HttpDelete("level/{levelId}")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> DeleteLevel(int levelId)
        {
            var ok = await _service.DeleteLevelAsync(levelId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("register-student")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDTO dto)
        {
            var ok = await _service.RegisterStudentToLevelAsync(dto);
            if (!ok) return BadRequest("لا يمكن تسجيل الطالب. ربما تم تسجيله بالفعل في مستوى آخر من هذه المسابقة.");
            return Ok();
        }

        [HttpDelete("level/{levelId}/student/{studentId}")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> UnregisterStudent(int levelId, int studentId)
        {
            var ok = await _service.UnregisterStudentFromLevelAsync(levelId, studentId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("level/{levelId}/results")]
        [Authorize(Policy = Permissions.ViewCompetitions)]
        public async Task<IActionResult> GetLevelResults(int levelId)
        {
            var res = await _service.GetLevelResultsAsync(levelId);
            return Ok(res);
        }

        [HttpPut("level/{levelId}/results")]
        [Authorize(Policy = Permissions.ManageCompetitions)]
        public async Task<IActionResult> SaveResults(int levelId, [FromBody] SaveCompetitionResultsDTO dto)
        {
            try
            {
                var ok = await _service.SaveLevelResultsAsync(levelId, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetStudentCompetitions(int studentId)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (studentIdClaim != studentId.ToString())
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && 
                     !User.HasClaim("Permission", Permissions.ViewCompetitions) && 
                     !User.HasClaim("Permission", Permissions.ManageCompetitions))
            {
                return Forbid();
            }

            var res = await _service.GetStudentCompetitionsAsync(studentId);
            return Ok(res);
        }
    }
}
