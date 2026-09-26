using DarV2.DTOs.Role;
using DarV2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DarV2.Service
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<RoleDTO>> GetAllAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var dtos = new List<RoleDTO>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                dtos.Add(new RoleDTO
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList()
                });
            }

            return dtos;
        }

        public async Task<RoleDTO?> GetByIdAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            var claims = await _roleManager.GetClaimsAsync(role);
            return new RoleDTO
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList()
            };
        }

        public async Task<bool> CreateAsync(RoleAddDTO dto)
        {
            if (await _roleManager.RoleExistsAsync(dto.Name)) return false;

            var role = new IdentityRole(dto.Name);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                foreach (var permission in dto.Permissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateAsync(string id, RoleAddDTO dto)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return false;

            // Update Name
            role.Name = dto.Name;
            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded) return false;

            // Update Permissions
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims.Where(c => c.Type == "Permission").ToList();

            // Remove old
            foreach (var claim in existingPermissions)
            {
                if (!dto.Permissions.Contains(claim.Value))
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            // Add new
            var existingPermissionValues = existingPermissions.Select(c => c.Value).ToList();
            foreach (var permission in dto.Permissions)
            {
                if (!existingPermissionValues.Contains(permission))
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return false;

            // Prevent deleting Admin role or similar protections can be added here
            if (role.Name == "Admin") return false;

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }
    }
}
