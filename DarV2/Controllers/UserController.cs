using DarV2.Models;
using DarV2.Service;
using DarV2.DTOs;
using DarV2.DTOs.AppUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewUsers)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _userService.GetTeachersAsync();
            return Ok(teachers);
        }

        [HttpPut("{userId}/roles")]
        [Authorize(Policy = Permissions.ManageUsers)]
        public async Task<IActionResult> AssignRoles(string userId, [FromBody] AssignRolesDTO dto)
        {
            var success = await _userService.AssignRolesAsync(userId, dto.Roles);
            if (!success) return BadRequest("Failed to assign roles.");
            return NoContent();
        }

        // ── Profile Endpoints ───────────────────────────────────────────────

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _userService.GetProfileAsync(userId);
            if (profile == null) return NotFound("User not found.");
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var (success, error, profile) = await _userService.UpdateProfileAsync(userId, dto);
            if (!success) return BadRequest(new { message = error });
            return Ok(profile);
        }

        [HttpPost("profile/photo")]
        public async Task<IActionResult> UpdateProfilePhoto(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var (success, error, photoUrl) = await _userService.UpdateProfilePhotoAsync(userId, file);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { profilePictureUrl = photoUrl });
        }

        [HttpDelete("profile/photo")]
        public async Task<IActionResult> RemoveProfilePhoto()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _userService.RemoveProfilePhotoAsync(userId);
            if (!success) return BadRequest("Failed to remove photo.");
            return NoContent();
        }

        [HttpPut("profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var (success, error) = await _userService.ChangePasswordAsync(userId, dto);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
        }
    }
}
