using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeAI.AI.Enums;

namespace ResumeAI.AI.Entities;

[Table("ai_requests")]
public class AiRequest
{
    [Key]
    [Column("request_id")]
    [MaxLength(36)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("resume_id")]
    public int ResumeId { get; set; }

    [Required]
    [Column("request_type")]
    public RequestType RequestType { get; set; }

    // Sanitised prompt sent to the AI model
    [Column("input_prompt", TypeName = "text")]
    public string? InputPrompt { get; set; }

    // Full response received from the AI model
    [Column("ai_response", TypeName = "text")]
    public string? AiResponse { get; set; }

    [Column("model")]
    public AiModel Model { get; set; } = AiModel.GPT4O;

    [Column("tokens_used")]
    public int TokensUsed { get; set; } = 0;

    [Column("status")]
    public RequestStatus Status { get; set; } = RequestStatus.QUEUED;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
