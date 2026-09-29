using CloudinaryDotNet;
using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DarV2.Service
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _config;

        public StudentService(IUnitOfWork uow, Cloudinary cloudinary, IConfiguration config)
        {
            _uow = uow;
            _cloudinary = cloudinary;
            _config = config;
        }

        public async Task<StudentPagedResultDTO> GetAllAsync(int page = 1, int pageSize = 20, int? academicYearId = null, int? groupId = null, string? search = null, bool? isActive = null, int? gender = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _uow.Students.Query()
                .AsNoTracking()
                .Include(s => s.AcademicYear)
                .Include(s => s.Images)
                .Include(s => s.MemorizationRecords)
                .Include(s => s.StudentGroups)
                .Include(s => s.Phones)
                .AsSplitQuery()
                .AsQueryable();

            if (academicYearId.HasValue)
                query = query.Where(s => s.AcademicYearId == academicYearId.Value);

            if (groupId.HasValue)
                query = query.Where(s => s.StudentGroups.Any(sg => sg.GroupId == groupId.Value));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(term) || (s.SSN != null && s.SSN.Contains(term)) || s.Code.ToLower().Contains(term));
            }

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var maleCount = await query.CountAsync(s => s.Gender == Gender.Male);
            var femaleCount = await query.CountAsync(s => s.Gender == Gender.Female);

            if (gender.HasValue)
                query = query.Where(s => s.Gender == (Gender)gender.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(s => new StudentDetailsDTO
            {
                Id = s.Id,
                FullName = s.FullName,
                SSN = s.SSN,
                IsActive = s.IsActive,
                Notes = s.Notes,
                Gender = s.Gender,
                Code=s.Code,
                Password = s.PlainPassword ?? s.SSN,
                AcademicYear = s.AcademicYear != null ? new AcademicYearViewDTO { Id = s.AcademicYear.Id, Name = s.AcademicYear.Name, TypeSchool = (int)s.AcademicYear.TypeSchool } : null,
                MemorizationRecords = s.MemorizationRecords.Select(mr => new MemorizationRecordDTO
                {
                    Id = mr.Id,
                    StudentId = mr.StudentId,
                    StudentName = mr.Student.FullName,
                    FromSurahId = mr.FromSurahId,
                    FromAyah = mr.FromAyah,
                    ToSurahId = mr.ToSurahId,
                    ToAyah = mr.ToAyah,
                    Date = mr.Date,
                    Notes = mr.Notes
                }).ToList(),
                Images = s.Images.Select(i => new ImageViewDTO { Id = i.Id, Url = i.Url }).ToList(),
                Phones = s.Phones.Select(p => new PhoneViewDTO { Id = p.Id, Number = p.Number }).ToList()
            }).ToList();

            return new StudentPagedResultDTO
            {
                Items = dtos,
                TotalCount = total,
                MaleCount = maleCount,
                FemaleCount = femaleCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<StudentDetailsDTO?> GetByIdAsync(int id)
        {
            var student = await _uow.Students.GetStudentAsync(id);
            if (student == null) return null;

            var dto = new StudentDetailsDTO
            {
                Id = student.Id,
                FullName = student.FullName,
                SSN = student.SSN,
                Notes = student.Notes,
                Gender = student.Gender,
                IsActive= student.IsActive,
                Code = student.Code,
                Password = student.PlainPassword ?? student.SSN,
                MemorizationRecords = student.MemorizationRecords.Select(mr => new MemorizationRecordDTO
                {
                    Id = mr.Id,
                    StudentId = mr.StudentId,
                    StudentName = mr.Student.FullName,
                    FromSurahId = mr.FromSurahId,
                    FromAyah = mr.FromAyah,
                    ToSurahId = mr.ToSurahId,
                    ToAyah = mr.ToAyah,
                    Date = mr.Date,
                    Notes = mr.Notes
                }).ToList(),
                Groups= student.StudentGroups.Select(g => new GroupCardDTO
                {
                    Id = g.Id,
                    Name = g.Group.Name,
                    Description = g.Group.Description,
                    TeacherName = g.Group.Teacher?.UserName,
                    StudentCount = g.Group.StudentGroups?.Count ?? 0
                }).ToList(),
                AcademicYear = student.AcademicYear != null ? new AcademicYearViewDTO
                {
                    Id = student.AcademicYear.Id,
                    Name = student.AcademicYear.Name,
                    TypeSchool = (int)student.AcademicYear.TypeSchool
                } : null,
                Images = student.Images.Select(i => new ImageViewDTO { Id = i.Id, Url = i.Url }).ToList(),
                Phones = student.Phones.Select(p => new PhoneViewDTO { Id = p.Id, Number = p.Number }).ToList()
            };

            return dto;
        }

        public async Task<IEnumerable<GroupCardDTO>> GetGroupsAsync(int studentId)
        {
            var student = await _uow.Students.GetStudentPage(studentId);
            if (student == null) return Enumerable.Empty<GroupCardDTO>();

            var groups = student.StudentGroups.Select(sg => sg.Group).ToList();
            return groups.Select(g => new GroupCardDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                TeacherName = g.Teacher?.FullName,
                StudentCount = g.StudentGroups?.Count ?? 0
            }).ToList();
        }

        public async Task<StudentDetailsDTO> CreateAsync(StudentAddDTO dto)
        {
            var lastCode = await _uow.Students
                .Query()
                .OrderByDescending(s => s.Id)
                .Select(s => s.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastCode is not null)
            {
                var numberPart = lastCode.Replace("STD-", "");
                if (int.TryParse(numberPart, out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            var code = $"STD-{nextNumber:D4}";

            var codeExists = await _uow.Students.Query().AnyAsync(s => s.Code == code);
            if (codeExists)
                throw new InvalidOperationException($"Code {code} already exists, try again");

            var student = new Student
            {
                FullName = dto.FullName,
                SSN = dto.SSN,
                Notes = dto.Notes,
                Gender = dto.Gender,
                AcademicYearId = dto.AcademicYearId,
                Code = code,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.SSN),
                PlainPassword = dto.SSN
            };

            await _uow.Students.AddAsync(student);
            await _uow.SaveAsync();

            if (dto.GroupIds != null)
            {
                foreach (var gid in dto.GroupIds)
                {
                    await _uow.StudentGroups.AddAsync(new StudentGroup { StudentId = student.Id, GroupId = gid });
                }
            }

            if (dto.ImageFiles != null)
            {
                foreach (var file in dto.ImageFiles)
                {
                    var result = await FileUpload.UploadAsync(file, _cloudinary);
                    var url = result.Url?.ToString() ?? string.Empty;
                    await _uow.Images.AddAsync(new Image { Url = url, StudentId = student.Id });
                }
            }

            if (dto.PhoneNumbers != null)
            {
                foreach (var number in dto.PhoneNumbers)
                {
                    await _uow.Phones.AddAsync(new Phone { Number = number, StudentId = student.Id });
                }
            }

            await _uow.SaveAsync();

            return await GetByIdAsync(student.Id) ?? throw new InvalidOperationException();
        }

        public async Task<bool> UpdateAsync(int id, StudentUpdateDTO dto)
        {
            var student = await _uow.Students.GetByIdAsync(id);
            if (student == null) return false;

            student.FullName = dto.FullName;
            student.SSN = dto.SSN;
            student.Notes = dto.Notes;
            student.Gender = dto.Gender;
            if (dto.AcademicYearId.HasValue)
            {
                student.AcademicYearId = dto.AcademicYearId.Value;
            }

            _uow.Students.Update(student);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _uow.Students.GetByIdAsync(id);
            if (student == null) return false;

            _uow.Students.Remove(student);
            await _uow.SaveAsync();
            return true;
        }

        #region Image Management
        public async Task<bool> AddImageAsync(int studentId, List<IFormFile> files)
        {
            var student = await _uow.Students.GetByIdAsync(studentId);
            if (student == null) throw new InvalidOperationException("Student not found");

            if (files != null)
            {
                var uploadTasks = files.Select(file => FileUpload.UploadAsync(file, _cloudinary));
                var results = await Task.WhenAll(uploadTasks);

                foreach (var result in results)
                {
                    var img = new Image { Url = result.Url.ToString(), StudentId = studentId };
                    await _uow.Images.AddAsync(img);
                }
                await _uow.SaveAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveImageAsync(int imageId)
        {
            var img = await _uow.Images.GetByIdAsync(imageId);
            if (img == null) return false;

            await FileUpload.DeleteImageAsync(img.Url, _cloudinary);

            _uow.Images.Remove(img);
            await _uow.SaveAsync();
            return true;
        }
        #endregion

        #region Phone Management
        public async Task<bool> CreatePhoneAsync(int studentId, string number)
        {
            var student = await _uow.Students.GetStudentAsync(studentId);
            if (student == null) throw new Exception("Ø§Ù„Ø·Ø§Ù„Ø¨ ØºÙŠØ± Ù…Ø³Ø¬Ù„ ");
            student.Phones.Add(new Phone { Number = number });
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeletePhoneAsync(int phoneId)
        {
            var phone = await _uow.Phones.GetByIdAsync(phoneId);
            if (phone == null) throw new Exception("Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯");
            _uow.Phones.Remove(phone);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> UpdatePhoneAsync(int phoneId, string number)
        {
            var phone = await _uow.Phones.GetByIdAsync(phoneId);
            if (phone == null) throw new Exception("Ø±Ù‚Ù… Ø§Ù„Ù‡Ø§ØªÙ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯");
            phone.Number = number;
            _uow.Phones.Update(phone);
            await _uow.SaveAsync();
            return true;
        }
        #endregion 

        public async Task<bool> AssigenGroupToStudent(int studentId,int groupId)
        {
            var studentExists = await _uow.Students
        .Query()
        .AnyAsync(s => s.Id == studentId);

            if (!studentExists)
                return false;

            var groupExists = await _uow.Groups
                .Query()
                .AnyAsync(g => g.Id == groupId);

            if (!groupExists)
                return false;

            var alreadyAssigned = await _uow.StudentGroups
                .Query()
                .AnyAsync(sg =>
                    sg.StudentId == studentId &&
                    sg.GroupId == groupId);

            if (alreadyAssigned)
                return false;

            await _uow.StudentGroups.AddAsync(new StudentGroup
            {
                StudentId = studentId,
                GroupId = groupId
            });

            await _uow.SaveAsync();

            return true;
        }

        public async Task<bool> UnAssigenGroupToStudent(int studentId, int groupId)
        {
            
            var alreadyAssigned = await _uow.StudentGroups
                .Query()
                .AnyAsync(sg =>
                    sg.StudentId == studentId &&
                    sg.GroupId == groupId);

            if (!alreadyAssigned)
                return false;

            await _uow.StudentGroups.Query()
                .Where(sg => sg.StudentId == studentId && sg.GroupId == groupId)
                .ForEachAsync(sg => _uow.StudentGroups.Remove(sg));

            await _uow.SaveAsync();

            return true;
        }

        public async Task<bool> ValidSSNAsync(string ssn)
        {
            return await _uow.Students.Query().AnyAsync(s => s.SSN == ssn);
        }

        public async Task<StudentLoginResponse> LoginAsync(string Code, string Password)
        {
            Code = Code?.Trim() ?? string.Empty;
            Password = Password?.Trim() ?? string.Empty;

            var student = await _uow.Students
                                    .FirstOrDefaultAsync(s => s.Code == Code
                                                           && s.IsActive);

            if (student is null)
                throw new UnauthorizedAccessException("Invalid code or password");

            bool valid = false;

            if (string.IsNullOrWhiteSpace(student.PasswordHash))
            {
                // Fallback to checking student's SSN if no password hash is set yet
                if (!string.IsNullOrWhiteSpace(student.SSN) && student.SSN.Trim() == Password)
                {
                    valid = true;
                    student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                }
            }
            else
            {
                try
                {
                    valid = BCrypt.Net.BCrypt.Verify(Password, student.PasswordHash);
                }
                catch
                {
                    // Fallback if PasswordHash in DB was plaintext or malformed
                    if (student.PasswordHash == Password || (!string.IsNullOrWhiteSpace(student.SSN) && student.SSN.Trim() == Password))
                    {
                        valid = true;
                        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                    }
                }
            }

            if (!valid)
                throw new UnauthorizedAccessException("Invalid code or password");

            var token = GenerateJwtToken(student);
            var refreshToken = GenerateRefreshToken();

            student.RefreshToken = refreshToken;
            student.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            _uow.Students.Update(student);
            await _uow.SaveAsync();

            return new StudentLoginResponse
            {
                StudentId = student.Id,
                Token = token,
                RefreshToken = refreshToken,
                FullName = student.FullName,
                Code = student.Code,
                Role = "Student",
                Roles = new List<string> { "Student" }
            };
        }

        public async Task<StudentLoginResponse?> RefreshStudentTokenAsync(RefreshTokenRequestDTO model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null) return null;

            var studentIdClaim = principal.FindFirst("studentId");
            if (studentIdClaim == null || !int.TryParse(studentIdClaim.Value, out int studentId)) 
                return null;

            var student = await _uow.Students.GetByIdAsync(studentId);
            if (student == null || student.RefreshToken != model.RefreshToken || student.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var newToken = GenerateJwtToken(student);
            var newRefreshToken = GenerateRefreshToken();

            student.RefreshToken = newRefreshToken;
            student.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _uow.Students.Update(student);
            await _uow.SaveAsync();

            return new StudentLoginResponse
            {
                StudentId = student.Id,
                Token = newToken,
                RefreshToken = newRefreshToken,
                FullName = student.FullName,
                Code = student.Code,
                Role = "Student"
            };
        }

        public async Task ChangePasswordAsync(int studentId, string currentPassword, string newPassword)
        {
            var student = await _uow.Students.GetByIdAsync(studentId)
                ?? throw new ArgumentException("Student not found");

            currentPassword = currentPassword?.Trim() ?? string.Empty;
            newPassword = newPassword?.Trim() ?? string.Empty;

            bool currentValid = false;
            if (string.IsNullOrWhiteSpace(student.PasswordHash))
            {
                currentValid = !string.IsNullOrWhiteSpace(student.SSN) && student.SSN.Trim() == currentPassword;
            }
            else
            {
                try
                {
                    currentValid = BCrypt.Net.BCrypt.Verify(currentPassword, student.PasswordHash);
                }
                catch
                {
                    currentValid = student.PasswordHash == currentPassword || (!string.IsNullOrWhiteSpace(student.SSN) && student.SSN.Trim() == currentPassword);
                }
            }

            if (!currentValid)
                throw new UnauthorizedAccessException("Current password is incorrect");

            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            student.PlainPassword = newPassword;

            _uow.Students.Update(student);
            await _uow.SaveAsync();
        }

        private string GenerateJwtToken(Student student)
        {
            var claims = new[]
            {
            new Claim("studentId", student.Id.ToString()),
            new Claim("code",      student.Code),
            new Claim(ClaimTypes.Name, student.FullName),
            new Claim(ClaimTypes.Role, "Student"),
        };

            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    
        public async Task<(bool success, string? error, string? newPassword)> ResetPasswordByAdminAsync(int studentId, string? newPassword)
        {
            var student = await _uow.Students.GetStudentAsync(studentId);
            if (student == null) return (false, "الطالب غير موجود.", null);

            var pass = string.IsNullOrWhiteSpace(newPassword) ? student.SSN : newPassword.Trim();
            if (string.IsNullOrWhiteSpace(pass))
            {
                return (false, "لا يمكن تعيين كلمة مرور فارغة.", null);
            }

            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass);
            student.PlainPassword = pass;
            _uow.Students.Update(student);
            await _uow.SaveAsync();

            return (true, null, pass);
        }
    }
}
