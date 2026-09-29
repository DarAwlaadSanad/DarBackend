using CloudinaryDotNet;
using DarV2.DTOs;
using DarV2.DTOs.AppUser;
using DarV2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using DarV2.Context;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Cloudinary _cloudinary;
        private readonly DarContext _context;

        public UserService(UserManager<ApplicationUser> userManager, Cloudinary cloudinary, DarContext context)
        {
            _userManager = userManager;
            _cloudinary = cloudinary;
            _context = context;
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
                    IsActive = u.IsActive,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    Gender = u.Gender,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public async Task<IEnumerable<UserViewDTO>> GetTeachersAsync()
        {
            var teacherRoles = new[] { "مدرس", "Teacher", "معلم", "مشرف" };
            var usersMap = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);

            foreach (var role in teacherRoles)
            {
                try
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    foreach (var u in usersInRole)
                    {
                        usersMap[u.Id] = u;
                    }
                }
                catch { }
            }

            // Also include any user who is currently assigned as a teacher in any Group
            try
            {
                var assignedTeacherIds = await _context.Groups
                    .Where(g => !string.IsNullOrEmpty(g.TeacherId))
                    .Select(g => g.TeacherId!)
                    .Distinct()
                    .ToListAsync();

                foreach (var tId in assignedTeacherIds)
                {
                    if (!usersMap.ContainsKey(tId))
                    {
                        var u = await _userManager.FindByIdAsync(tId);
                        if (u != null)
                        {
                            usersMap[u.Id] = u;
                        }
                    }
                }
            }
            catch { }

            // Fallback: If still empty, return all non-sysadmin users
            if (usersMap.Count == 0)
            {
                var fallbackUsers = await _userManager.Users
                    .Where(u => u.UserName != "sysadmin")
                    .ToListAsync();
                foreach (var u in fallbackUsers)
                {
                    usersMap[u.Id] = u;
                }
            }

            var result = new List<UserViewDTO>();
            foreach (var u in usersMap.Values.OrderBy(x => x.FullName ?? x.UserName))
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserViewDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    Gender = u.Gender,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        private static readonly HashSet<string> SystemRoles = new() { "Admin", "User" };

        public async Task<bool> AssignRolesAsync(string userId, List<string> customRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var systemRolesToKeep = currentRoles.Where(r => SystemRoles.Contains(r)).ToList();
            var validCustomRoles = customRoles.Where(r => !SystemRoles.Contains(r)).Distinct().ToList();
            var targetRoles = systemRolesToKeep.Concat(validCustomRoles).Distinct().ToList();

            var toRemove = currentRoles.Except(targetRoles).ToList();
            var toAdd = targetRoles.Except(currentRoles).ToList();

            if (toRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded) return false;
            }

            if (toAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addResult.Succeeded) return false;
            }

            return true;
        }

        public async Task<UserProfileDTO?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserProfileDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Gender = user.Gender,
                Roles = roles.ToList()
            };
        }

        public async Task<(bool success, string? error, UserProfileDTO? profile)> UpdateProfileAsync(string userId, UpdateProfileDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "المستخدم غير موجود", null);

            // Check email uniqueness if changed
            if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != user.Id)
                {
                    return (false, "البريد الإلكتروني مستخدم بالفعل بحساب آخر", null);
                }
                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpperInvariant();
            }

            // Check username uniqueness if changed
            if (!string.IsNullOrWhiteSpace(dto.UserName) && !dto.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return (false, "اسم المستخدم مستخدم بالفعل بحساب آخر", null);
                }
                user.UserName = dto.UserName;
                user.NormalizedUserName = dto.UserName.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName.Trim();
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var err = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return (false, err, null);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var profile = new UserProfileDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Gender = user.Gender,
                Roles = roles.ToList()
            };
            return (true, null, profile);
        }

        public async Task<(bool success, string? error, string? photoUrl)> UpdateProfilePhotoAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "المستخدم غير موجود", null);

            if (file == null || file.Length == 0) return (false, "الملف غير صالح", null);

            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
            {
                return (false, "نوع الملف غير مدعوم، يرجى رفع صورة (jpg, png, webp)", null);
            }

            // Delete old photo from Cloudinary if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.Contains("res.cloudinary.com"))
            {
                try
                {
                    await FileUpload.DeleteImageAsync(user.ProfilePictureUrl, _cloudinary);
                }
                catch { }
            }

            var uploadResult = await FileUpload.UploadAsync(file, _cloudinary);
            if (uploadResult.Error != null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
            {
                return (false, uploadResult.Error?.Message ?? "فشل في رفع الصورة", null);
            }

            user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
            await _userManager.UpdateAsync(user);

            return (true, null, user.ProfilePictureUrl);
        }

        public async Task<bool> RemoveProfilePhotoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.Contains("res.cloudinary.com"))
            {
                try
                {
                    await FileUpload.DeleteImageAsync(user.ProfilePictureUrl, _cloudinary);
                }
                catch { }
            }

            user.ProfilePictureUrl = null;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<(bool success, string? error)> ChangePasswordAsync(string userId, ChangePasswordDTO dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return (false, "كلمة المرور الجديدة وتأكيدها غير متطابقين");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "المستخدم غير موجود");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var err = string.Join(", ", result.Errors.Select(e => e.Description));
                if (err.Contains("Incorrect password")) err = "كلمة المرور الحالية غير صحيحة";
                return (false, err);
            }

            return (true, null);
        }
    
        public async Task<(bool success, string? error, bool? newStatus)> ToggleStatusAsync(string currentUserId, string targetUserId)
        {
            if (currentUserId == targetUserId)
            {
                return (false, "لا يمكنك تغيير حالة حسابك الحالي.", null);
            }

            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) return (false, "المستخدم غير موجود.", null);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
            {
                return (false, "لا يمكن تعطيل حساب مدير النظام الرئيسي (SuperAdmin).", null);
            }

            user.IsActive = !user.IsActive;
            if (!user.IsActive)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
            }

            return (true, null, user.IsActive);
        }

        public async Task<(bool success, string? error)> DeleteUserAsync(string currentUserId, string targetUserId)
        {
            if (currentUserId == targetUserId)
            {
                return (false, "لا يمكنك حذف حسابك الحالي.");
            }

            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) return (false, "المستخدم غير موجود.");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                return (false, "لا يمكن حذف حساب المسؤول أو مدير النظام.");
            }

            var hasGroups = await _context.Groups.AnyAsync(g => g.TeacherId == targetUserId);
            if (hasGroups)
            {
                return (false, "لا يمكن حذف هذا المعلم لأنه مرتبط بحلقات كمعلم أساسي. يمكنك نقل حلقاته أولاً أو وضعه كـ غير نشط لمنعه من تسجيل الدخول.");
            }

            var hasAttendance = await _context.TeacherAttendances.AnyAsync(a => a.TeacherId == targetUserId);
            var hasSessions = await _context.Sessions.AnyAsync(s => s.SubstituteTeacherId == targetUserId);
            if (hasAttendance || hasSessions)
            {
                return (false, "لا يمكن حذف هذا المستخدم نهائياً لوجود سجلات حضور وحصص مرتبطة به. يمكنك وضعه كـ غير نشط لتعطيل حسابه ومنعه من تسجيل الدخول.");
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "تعذر حذف المستخدم لوجود ارتباطات في قاعدة البيانات: " + ex.Message);
            }
        }
    }
}
