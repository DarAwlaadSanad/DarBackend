using Microsoft.AspNetCore.Identity;

namespace DarV2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Gender? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Teacher → Groups
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
