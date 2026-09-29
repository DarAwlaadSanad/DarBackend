using DarV2.Context;
using DarV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DarV2
{
    public static class BookSeeder
    {
        public static async Task SeedBooksAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DarContext>();

            if (!await context.Books.AnyAsync())
            {
                var books = new[]
                {
                    new Book
                    {
                        Title = "ØªØ­ÙØ© Ø§Ù„Ø£Ø·ÙØ§Ù„ ÙˆØ§Ù„ØºÙ„Ù…Ø§Ù† ÙÙŠ ØªØ¬ÙˆÙŠØ¯ Ø§Ù„Ù‚Ø±Ø¢Ù†",
                        Author = "Ø§Ù„Ø´ÙŠØ® Ø³Ù„ÙŠÙ…Ø§Ù† Ø§Ù„Ø¬Ù…Ø²ÙˆØ±ÙŠ Ø±Ø­Ù…Ù‡ Ø§Ù„Ù„Ù‡",
                        Category = "ØªØ¬ÙˆÙŠØ¯ ÙˆÙ…ØªÙˆÙ†",
                        TargetRole = "All",
                        DriveUrl = "https://drive.google.com/file/d/1XyZ_example_tohfah/view",
                        CoverUrl = "assets/images/book-covers/tohfah.svg",
                        Description = "Ù…Ù†Ø¸ÙˆÙ…Ø© Ø´Ø¹Ø±ÙŠØ© Ù…ÙŠØ³Ø±Ø© Ù„Ù„Ù…Ø¨ØªØ¯Ø¦ÙŠÙ† ÙÙŠ Ø¹Ù„Ù… Ø§Ù„ØªØ¬ÙˆÙŠØ¯ØŒ ØªØªÙ†Ø§ÙˆÙ„ Ø£Ø­ÙƒØ§Ù… Ø§Ù„Ù†ÙˆÙ† Ø§Ù„Ø³Ø§ÙƒÙ†Ø© ÙˆØ§Ù„ØªÙ†ÙˆÙŠÙ† ÙˆØ§Ù„Ù…ÙŠÙ… Ø§Ù„Ø³Ø§ÙƒÙ†Ø© ÙˆØ£Ø­ÙƒØ§Ù… Ø§Ù„Ù…Ø¯ÙˆØ¯ ÙˆÙ…Ø®Ø§Ø±Ø¬ Ø§Ù„Ø­Ø±ÙˆÙ.",
                        PagesCount = 24,
                        FileSize = "1.2 Ù…ÙŠØ¬Ø§Ø¨Ø§ÙŠØª",
                        ViewsCount = 142,
                        DownloadsCount = 88,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                await context.Books.AddRangeAsync(books);
                await context.SaveChangesAsync();
            }
        }
    }
}