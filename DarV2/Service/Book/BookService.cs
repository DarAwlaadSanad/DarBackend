using CloudinaryDotNet;
using DarV2.Context;
using DarV2.DTOs.Book;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarV2.Service.Book
{
    public class BookService : IBookService
    {
        private readonly DarContext _context;
        private readonly Cloudinary _cloudinary;

        public BookService(DarContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<List<BookDTO>> GetAllAsync(string? category = null, string? targetRole = null, string? search = null)
        {
            var query = _context.Books.AsNoTracking().Where(b => b.IsActive);

            if (!string.IsNullOrWhiteSpace(category) && category != "Ø§Ù„ÙƒÙ„")
            {
                query = query.Where(b => b.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(targetRole) && targetRole != "All" && targetRole != "Ø§Ù„ÙƒÙ„")
            {
                // Return books targeted to 'All' or to the specific user role
                query = query.Where(b => b.TargetRole == "All" || b.TargetRole == "Ø§Ù„ÙƒÙ„" || b.TargetRole.ToLower() == targetRole.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(s) || 
                                         (b.Author != null && b.Author.ToLower().Contains(s)) ||
                                         (b.Description != null && b.Description.ToLower().Contains(s)) ||
                                         b.Category.ToLower().Contains(s));
            }

            var books = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            return books.Select(b => MapToDTO(b)).ToList();
        }

        public async Task<BookDTO?> GetByIdAsync(int id)
        {
            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
            return book != null ? MapToDTO(book) : null;
        }

        public async Task<BookDTO> CreateAsync(BookCreateDTO dto)
        {
            string? coverUrl = dto.CoverUrl;

            // Upload image file if provided
            if (dto.CoverFile != null && dto.CoverFile.Length > 0)
            {
                var uploadResult = await FileUpload.UploadAsync(dto.CoverFile, _cloudinary);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    coverUrl = uploadResult.SecureUrl.ToString();
                }
            }

            var book = new Models.Book
            {
                Title = dto.Title.Trim(),
                Author = dto.Author?.Trim(),
                Category = dto.Category.Trim(),
                TargetRole = string.IsNullOrWhiteSpace(dto.TargetRole) ? "All" : dto.TargetRole.Trim(),
                DriveUrl = dto.DriveUrl.Trim(),
                CoverUrl = coverUrl,
                Description = dto.Description?.Trim(),
                PagesCount = dto.PagesCount,
                FileSize = dto.FileSize?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ViewsCount = 0,
                DownloadsCount = 0
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            return MapToDTO(book);
        }

        public async Task<bool> UpdateAsync(int id, BookUpdateDTO dto)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
            if (book == null) return false;

            book.Title = dto.Title.Trim();
            book.Author = dto.Author?.Trim();
            book.Category = dto.Category.Trim();
            book.TargetRole = string.IsNullOrWhiteSpace(dto.TargetRole) ? "All" : dto.TargetRole.Trim();
            book.DriveUrl = dto.DriveUrl.Trim();
            book.Description = dto.Description?.Trim();
            book.PagesCount = dto.PagesCount;
            book.FileSize = dto.FileSize?.Trim();

            // Handle cover update
            if (dto.CoverFile != null && dto.CoverFile.Length > 0)
            {
                var uploadResult = await FileUpload.UploadAsync(dto.CoverFile, _cloudinary);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    book.CoverUrl = uploadResult.SecureUrl.ToString();
                }
            }
            else if (!string.IsNullOrWhiteSpace(dto.CoverUrl))
            {
                book.CoverUrl = dto.CoverUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            // Soft delete or hard delete: we can mark IsActive = false or remove
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task IncrementViewsAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                book.ViewsCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementDownloadsAsync(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                book.DownloadsCount++;
                await _context.SaveChangesAsync();
            }
        }

        private static BookDTO MapToDTO(Models.Book b)
        {
            return new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Category = b.Category,
                TargetRole = b.TargetRole,
                DriveUrl = b.DriveUrl,
                CoverUrl = b.CoverUrl,
                Description = b.Description,
                PagesCount = b.PagesCount,
                FileSize = b.FileSize,
                ViewsCount = b.ViewsCount,
                DownloadsCount = b.DownloadsCount,
                CreatedAt = b.CreatedAt
            };
        }
    }
}