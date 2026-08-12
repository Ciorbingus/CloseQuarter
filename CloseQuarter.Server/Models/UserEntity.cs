using System.ComponentModel.DataAnnotations;

namespace CloseQuarter.Server.Models;

public class UserEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? SessionToken { get; set; }
    public DateTime? SessionTokenExpiry { get; set; }

    public int TotalWins { get; set; } = 0;
    public int TotalLosses { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}