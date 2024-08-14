using System.ComponentModel.DataAnnotations;

namespace Server.Common.Models;

public class Account
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Status { get; set; }

    public List<RefreshToken> RefreshTokens { get; } = new();
}