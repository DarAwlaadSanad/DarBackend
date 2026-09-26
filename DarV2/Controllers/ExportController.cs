using DarV2.Models;
using DarV2.Service.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("students")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportStudents([FromQuery] int? groupId = null)
        {
            var fileBytes = await _exportService.ExportStudentsAsync(groupId);
            var fileName = groupId.HasValue ? $"Students_Group_{groupId}.xlsx" : "All_Students.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("students/attendance")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportGroupAttendance([FromQuery] int groupId, [FromQuery] int month, [FromQuery] int year)
        {
            var fileBytes = await _exportService.ExportGroupAttendanceAsync(groupId, month, year);
            var fileName = $"Attendance_Group_{groupId}_{month}_{year}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("teachers/attendance")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportTeacherAttendance([FromQuery] int month, [FromQuery] int year)
        {
            var fileBytes = await _exportService.ExportTeacherAttendanceAsync(month, year);
            var fileName = $"Teacher_Attendance_{month}_{year}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("finance/monthly")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportMonthlyFinance([FromQuery] int month, [FromQuery] int year)
        {
            var fileBytes = await _exportService.ExportMonthlyFinanceAsync(month, year);
            var fileName = $"Finance_Report_{month}_{year}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("groups-data")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportGroupsData()
        {
            var fileBytes = await _exportService.ExportGroupsDataAsync();
            var fileName = "بيانات_مجموعات_الدار.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("groups-template")]
        [Authorize(Policy = Permissions.ExportData)]
        public async Task<IActionResult> ExportGroupsTemplate()
        {
            var fileBytes = await _exportService.ExportEmptyTemplateAsync();
            var fileName = "نموذج_مجموعات_الدار.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost("import-groups")]
        [Authorize(Policy = Permissions.ImportData)]
        public async Task<IActionResult> ImportGroups(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("الملف غير صالح");
            try {
                var result = await _exportService.ImportGroupsAsync(file);
                if (result) return Ok(new { message = "تم استيراد المجموعات بنجاح" });
                return BadRequest("حدث خطأ أثناء استيراد الملف (الخدمة أرجعت false)");
            } catch (Exception ex) {
                return BadRequest($"حدث خطأ أثناء استيراد الملف: {ex.Message} {ex.InnerException?.Message}");
            }
        }
    }
}
