using DarV2.DTOs.Role;
using DarV2.Models;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // We can restrict this controller only to those who have the ManageRoles permission.
    // For now, checking if user has ManageRoles claim, or fallback to Admin role for safety.
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        private bool HasManageRolesPermission()
        {
            return User.HasClaim("Permission", Permissions.ManageRoles) || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!HasManageRolesPermission() && 
                !User.HasClaim("Permission", Permissions.ViewRoles) &&
                !User.HasClaim("Permission", Permissions.ViewUsers) &&
                !User.HasClaim("Permission", Permissions.ManageUsers) &&
                !User.IsInRole("مشرف") &&
                !User.IsInRole("Supervisor"))
                return Forbid();

            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (!HasManageRolesPermission() && !User.HasClaim("Permission", Permissions.ViewRoles))
                return Forbid();

            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpGet("permissions")]
        public IActionResult GetAllPermissions()
        {
            if (!HasManageRolesPermission()) return Forbid();

            return Ok(Permissions.GetAllPermissions());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleAddDTO dto)
        {
            if (!HasManageRolesPermission()) return Forbid();

            var success = await _roleService.CreateAsync(dto);
            if (!success) return BadRequest("Role creation failed or role already exists.");
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RoleAddDTO dto)
        {
            if (!HasManageRolesPermission()) return Forbid();

            var success = await _roleService.UpdateAsync(id, dto);
            if (!success) return BadRequest("Role update failed.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!HasManageRolesPermission()) return Forbid();

            var success = await _roleService.DeleteAsync(id);
            if (!success) return BadRequest("Role deletion failed or role is protected.");
            return NoContent();
        }
    }
}
