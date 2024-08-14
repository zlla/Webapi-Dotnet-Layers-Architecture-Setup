namespace Server.DTOs.Models.Auth
{
    public class Token
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}