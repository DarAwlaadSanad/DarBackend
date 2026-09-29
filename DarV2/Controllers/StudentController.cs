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
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewStudents)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? academicYearId = null, [FromQuery] int? groupId = null, [FromQuery] string? search = null, [FromQuery] bool? isActive = null, [FromQuery] int? gender = null)
        {
            var items = await _studentService.GetAllAsync(page, pageSize, academicYearId, groupId, search, isActive, gender);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (studentIdClaim != id.ToString())
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && 
                     !User.HasClaim("Permission", Permissions.ViewStudents) && 
                     !User.HasClaim("Permission", Permissions.ManageStudents))
            {
                return Forbid();
            }

            var item = await _studentService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("assainGroup")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> AssainGroup(int studentId,int groupId)
        {
            var ok = await _studentService.AssigenGroupToStudent(studentId, groupId);
            if (!ok) return BadRequest("الطالب بالفعل مسجل من قبل");
            return Ok("تم تسجيل الطالب في المجموعة");
        }
        [HttpPost("unAssainGroup")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> UnAssainGroup(int studentId, int groupId)
        {
            var ok = await _studentService.UnAssigenGroupToStudent(studentId, groupId);
            if (!ok) return BadRequest("الطالب غير مسجل في هذة المجموعة");
            return Ok("تم حذف الطالب من المجموعة");
        }

        [HttpGet("validate/{SSN}")]
        public async Task<IActionResult> ValidateSSN(string SSN)
        {
            var isValid = await _studentService.ValidSSNAsync(SSN);
            return Ok(new { isValid });
        }

        [HttpGet("{id}/groups")]
        [Authorize]
        public async Task<IActionResult> GetGroups(int id)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var studentIdClaim = User.FindFirst("studentId")?.Value;
                if (studentIdClaim != id.ToString())
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && 
                     !User.HasClaim("Permission", Permissions.ViewStudents) && 
                     !User.HasClaim("Permission", Permissions.ManageStudents))
            {
                return Forbid();
            }

            var groups = await _studentService.GetGroupsAsync(id);
            return Ok(groups);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> Create([FromForm] StudentAddDTO dto)
        {
            var created = await _studentService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDTO dto)
        {
            var ok = await _studentService.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("addPhone")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> AddPhone(int studetId,string phone)
        {
            var ok = await _studentService.CreatePhoneAsync(studetId, phone);
            if (!ok) return BadRequest();
            return Ok();
        }

        [HttpPut("updatePhone")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> UpdatePhone(int phoneId, string phone)
        {
            var ok = await _studentService.UpdatePhoneAsync(phoneId, phone);
            if (!ok) return BadRequest();
            return Ok();
        }
        [HttpDelete("deletePhone")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> DeletePhone(int phoneId)
        {
            var ok = await _studentService.DeletePhoneAsync(phoneId);
            if (!ok) return BadRequest();
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DeleteStudents)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _studentService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{studentId}/images")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> AddImage(int studentId, [FromForm] List<IFormFile> files)
        {
            var imgs = await _studentService.AddImageAsync(studentId, files);
            return CreatedAtAction(nameof(Get), new { id = studentId }, imgs);
        }

        [HttpDelete("images/{imageId}")]
        [Authorize(Policy = Permissions.ManageStudents)]
        public async Task<IActionResult> RemoveImage(int imageId)
        {
            var ok = await _studentService.RemoveImageAsync(imageId);
            if (!ok) return NotFound();
            return NoContent();
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(string Code, string Password)
        {
            try
            {
                var response = await _studentService.LoginAsync(Code, Password);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل الدخول: " + ex.Message });
            }
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.RefreshToken))
                return BadRequest("Invalid client request");

            var result = await _studentService.RefreshStudentTokenAsync(model);
            if (result == null) return Unauthorized("Invalid token or refresh token");

            return Ok(result);
        }

        [HttpPost("student-change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int studentId, string currentPassword, string newPassword)
        {
            var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
            if (isStudent)
            {
                var claimId = User.FindFirst("studentId")?.Value;
                if (claimId != studentId.ToString()) return Forbid();
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            await _studentService.ChangePasswordAsync(studentId, currentPassword, newPassword);
            return Ok(new { message = "Password changed successfully" });
        }


    
        [HttpPut("{id}/reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetStudentPasswordDTO dto)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin") && 
                !User.HasClaim("Permission", Permissions.ManageStudents) &&
                !User.HasClaim("Permission", Permissions.ViewStudentPasswords))
            {
                return Forbid();
            }

            var (success, error, newPassword) = await _studentService.ResetPasswordByAdminAsync(id, dto?.Password);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "تم تحديث كلمة مرور الطالب بنجاح.", password = newPassword });
        }
    }
}
