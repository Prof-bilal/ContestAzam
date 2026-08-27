namespace EventSphere.Api.Models;

// Direct user↔user messaging. Messaging is NOT the same as notifications:
// notifications are system→user; conversations are user→user via the API.

public class Conversation
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ConversationParticipant
{
    public int ConversationId { get; set; }
    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Conversation Conversation { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}

public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = null!;
    public AppUser Sender { get; set; } = null!;
}