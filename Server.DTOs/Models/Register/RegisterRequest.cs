namespace Server.DTOs.Models.Register
{
    public class RegisterRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}