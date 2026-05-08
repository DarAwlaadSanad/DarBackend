namespace DarV2.DTOs
{
    public class StudentLoginResponse
    {
        public int StudentId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}