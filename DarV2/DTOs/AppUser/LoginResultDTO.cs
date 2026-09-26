namespace DarV2.DTOs
{
    public class LoginResultDTO
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
