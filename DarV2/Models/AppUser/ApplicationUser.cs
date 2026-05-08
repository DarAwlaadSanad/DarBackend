using Microsoft.AspNetCore.Identity;

namespace DarV2.Models
{
  
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Teacher → Groups
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }

}
