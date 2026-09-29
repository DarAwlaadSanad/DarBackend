using Microsoft.AspNetCore.Http;
using System;

namespace DarV2.DTOs.Book
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string Category { get; set; } = string.Empty;
        public string TargetRole { get; set; } = "All";
        public string DriveUrl { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public int? PagesCount { get; set; }
        public string? FileSize { get; set; }
        public int ViewsCount { get; set; }
        public int DownloadsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string Category { get; set; } = string.Empty;
        public string TargetRole { get; set; } = "All";
        public string DriveUrl { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public IFormFile? CoverFile { get; set; }
        public string? Description { get; set; }
        public int? PagesCount { get; set; }
        public string? FileSize { get; set; }
    }

    public class BookUpdateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string Category { get; set; } = string.Empty;
        public string TargetRole { get; set; } = "All";
        public string DriveUrl { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public IFormFile? CoverFile { get; set; }
        public string? Description { get; set; }
        public int? PagesCount { get; set; }
        public string? FileSize { get; set; }
    }
}