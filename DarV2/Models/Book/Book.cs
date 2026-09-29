using System;

namespace DarV2.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Target role in system (e.g. "All", "Student", "Teacher", "Admin", etc.)
        /// </summary>
        public string TargetRole { get; set; } = "All";
        
        public string DriveUrl { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public int? PagesCount { get; set; }
        public string? FileSize { get; set; }
        
        public int ViewsCount { get; set; } = 0;
        public int DownloadsCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}