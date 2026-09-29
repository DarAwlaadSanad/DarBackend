namespace DarV2.DTOs
{
    public class UserViewDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public Models.Gender? Gender { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AssignRolesDTO
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}
