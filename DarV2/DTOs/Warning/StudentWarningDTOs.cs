using System;
using System.Collections.Generic;
using DarV2.Models;

namespace DarV2.DTOs
{
    public class StudentWarningCreateDTO
    {
        public int StudentId { get; set; }
        public WarningType WarningType { get; set; } = WarningType.Absence;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
        public int? GroupId { get; set; }
    }

    public class StudentWarningUpdateDTO
    {
        public WarningType WarningType { get; set; }
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
        public int? GroupId { get; set; }
    }

    public class StudentWarningViewDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public WarningType WarningType { get; set; }
        public string WarningTypeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentWarningSummaryDTO
    {
        public int TotalCount { get; set; }
        public int AbsenceCount { get; set; }      // ØºÙŠØ§Ø¨
        public int MisbehaviorCount { get; set; }  // Ø´ØºØ¨
        public int NotMemorizedCount { get; set; } // Ø¹Ø¯Ù… Ø­ÙØ¸
        public int OtherCount { get; set; }        // Ø£Ø®Ø±Ù‰
    }

    public class StudentWarningPagedResultDTO
    {
        public IEnumerable<StudentWarningViewDTO> Items { get; set; } = new List<StudentWarningViewDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 1));
    }
}