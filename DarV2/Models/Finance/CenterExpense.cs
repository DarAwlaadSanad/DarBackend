using System.ComponentModel.DataAnnotations;

namespace DarV2.Models.Finance
{
    public class CenterExpense
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public string? Notes { get; set; }
    }
}
