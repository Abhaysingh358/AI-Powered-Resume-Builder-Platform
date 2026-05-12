using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Column("role")]
    public Role Role { get; set; } = Role.USER;

    [Column("provider")]
    public AuthProvider Provider { get; set; } = AuthProvider.LOCAL;

    /// <summary>
    /// OAuth provider subject ID (Google sub / LinkedIn id).
    /// Null for LOCAL accounts.
    /// </summary>
    [MaxLength(255)]
    [Column("provider_id")]
    public string? ProviderId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("subscription_plan")]
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.FREE;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
