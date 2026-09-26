using System.Threading.Tasks;

namespace DarV2.Service.Export
{
    public interface IExportService
    {
        Task<byte[]> ExportStudentsAsync(int? groupId = null);
        Task<byte[]> ExportGroupAttendanceAsync(int groupId, int month, int year);
        Task<byte[]> ExportTeacherAttendanceAsync(int month, int year);
        Task<byte[]> ExportMonthlyFinanceAsync(int month, int year);
        Task<byte[]> ExportGroupsDataAsync();
        Task<byte[]> ExportEmptyTemplateAsync();
        Task<bool> ImportGroupsAsync(Microsoft.AspNetCore.Http.IFormFile file);
    }
}
