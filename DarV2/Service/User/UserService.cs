using DarV2.DTOs;
using DarV2.Models;
using Microsoft.AspNetCore.Identity;

namespace DarV2.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserViewDTO>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserViewDTO>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserViewDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public async Task<IEnumerable<UserViewDTO>> GetTeachersAsync()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Teacher");
            var result = new List<UserViewDTO>();
            foreach (var u in usersInRole)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserViewDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName,
                    Roles = roles.ToList()
                });
            }
            return result;
        }
    }
}
