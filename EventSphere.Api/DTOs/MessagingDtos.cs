using System.ComponentModel.DataAnnotations;

namespace EventSphere.Api.DTOs;

// ───────────────────────────── Messaging DTOs ─────────────────────────────

public class ConversationParticipantDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class ConversationDto
{
    public int Id { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public int? OtherUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class ConversationDetailDto
{
    public int Id { get; set; }
    public int? OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
}

public class CreateConversationRequest
{
    [Required(ErrorMessage = "Recipient is required.")]
    public int RecipientId { get; set; }
}

public class SendMessageRequest
{
    [Required(ErrorMessage = "Message content is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message cannot exceed 2000 characters.")]
    public string Content { get; set; } = string.Empty;
}