using System.ComponentModel.DataAnnotations;

namespace Server.Common.Models;

public class RefreshToken
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long AccountId { get; set; }
    [Required]
    public string? Value { get; set; }
    [Required]
    public DateTime ExpirationDate { get; set; }
    [Required]
    public bool Revoked { get; set; } = false;

    public Account? Account { get; set; } = null!;
    public List<AccessToken>? AccessTokens { get; } = new();
}