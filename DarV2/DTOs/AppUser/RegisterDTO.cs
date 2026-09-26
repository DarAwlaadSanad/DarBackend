namespace DarV2.DTOs
{
    public class RegisterDTO
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Models.Gender? Gender { get; set; }
    }
}
