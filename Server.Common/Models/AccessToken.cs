using System.ComponentModel.DataAnnotations;

namespace Server.Common.Models;

public class AccessToken
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long RefreshTokenId { get; set; }
    [Required]
    public string? Value { get; set; }
    [Required]
    public DateTime ExpirationDate { get; set; }
    [Required]
    public bool Revoked { get; set; } = false;

    public RefreshToken? RefreshToken { get; set; } = null!;
}