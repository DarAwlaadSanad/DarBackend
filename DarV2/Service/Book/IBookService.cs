using DarV2.DTOs.Book;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarV2.Service.Book
{
    public interface IBookService
    {
        Task<List<BookDTO>> GetAllAsync(string? category = null, string? targetRole = null, string? search = null);
        Task<BookDTO?> GetByIdAsync(int id);
        Task<BookDTO> CreateAsync(BookCreateDTO dto);
        Task<bool> UpdateAsync(int id, BookUpdateDTO dto);
        Task<bool> DeleteAsync(int id);
        Task IncrementViewsAsync(int id);
        Task IncrementDownloadsAsync(int id);
    }
}