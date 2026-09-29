using DarV2.DTOs.Book;
using DarV2.Service.Book;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? category = null, [FromQuery] string? targetRole = null, [FromQuery] string? search = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var isStudent = User.IsInRole("Student") || User.HasClaim(ClaimTypes.Role, "Student") || User.HasClaim("role", "Student");
                var isTeacher = User.IsInRole("Teacher") || User.IsInRole("Ù…Ø¯Ø±Ø³") || User.HasClaim(ClaimTypes.Role, "Teacher") || User.HasClaim("role", "Teacher") || User.HasClaim("role", "Ù…Ø¯Ø±Ø³");
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

                if (!isAdmin)
                {
                    if (isStudent && string.IsNullOrEmpty(targetRole))
                    {
                        targetRole = "Student";
                    }
                    else if (isTeacher && string.IsNullOrEmpty(targetRole))
                    {
                        targetRole = "Teacher";
                    }
                }
            }

            var books = await _bookService.GetAllAsync(category, targetRole, search);
            return Ok(books);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] BookCreateDTO dto)
        {
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isAdmin && !User.HasClaim("Permission", "Permissions.Library.Manage") && !User.HasClaim("Permission", "Permissions.Roles.Manage"))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.DriveUrl))
            {
                return BadRequest("Ø¹Ù†ÙˆØ§Ù† Ø§Ù„ÙƒØªØ§Ø¨ ÙˆØ±Ø§Ø¨Ø· Google Drive Ù…Ø·Ù„ÙˆØ¨Ø§Ù†");
            }

            var created = await _bookService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] BookUpdateDTO dto)
        {
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isAdmin && !User.HasClaim("Permission", "Permissions.Library.Manage") && !User.HasClaim("Permission", "Permissions.Roles.Manage"))
            {
                return Forbid();
            }

            var success = await _bookService.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isAdmin && !User.HasClaim("Permission", "Permissions.Library.Manage") && !User.HasClaim("Permission", "Permissions.Roles.Manage"))
            {
                return Forbid();
            }

            var success = await _bookService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordView(int id)
        {
            await _bookService.IncrementViewsAsync(id);
            return Ok();
        }

        [HttpPost("{id}/download")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordDownload(int id)
        {
            await _bookService.IncrementDownloadsAsync(id);
            return Ok();
        }
    }
}