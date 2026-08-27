using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Hubs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

/// <summary>
/// User↔user messaging. Membership is always enforced against the database; the
/// sender id is always the authenticated user, never a value from the request body.
/// Messages are the source of truth: SignalR only delivers a fresh, persisted copy.
/// </summary>
public interface IMessagingService
{
    Task<ConversationDetailDto?> GetOrCreateConversationAsync(int currentUserId, int recipientId);
    Task<List<ConversationDto>> GetMyConversationsAsync(int userId);
    Task<ConversationDetailDto?> GetConversationAsync(int conversationId, int userId);
    Task<MessageDto?> SendMessageAsync(int conversationId, int senderUserId, string content);
    Task<bool> MarkConversationReadAsync(int conversationId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<int> GetConversationUnreadCountAsync(int conversationId, int userId);
}

public class MessagingService : IMessagingService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<MessagingHub> _hub;

    public MessagingService(AppDbContext db, IHubContext<MessagingHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<ConversationDetailDto?> GetOrCreateConversationAsync(int senderUserId, int recipientUserId)
    {
        if (senderUserId == recipientUserId) return null;

        var existing = await _db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User).ThenInclude(u => u.UserDetails)
            .Include(c => c.Messages).ThenInclude(m => m.Sender)
            .Where(c => c.Participants.Any(p => p.UserId == senderUserId) &&
                        c.Participants.Any(p => p.UserId == recipientUserId))
            .FirstOrDefaultAsync();

        if (existing is not null) return await GetConversationAsync(existing.Id, senderUserId);

        var recipientExists = await _db.Users.AnyAsync(u => u.Id == recipientUserId && u.IsActive);
        if (!recipientExists) return null;

        var conversation = new Conversation
        {
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ConversationParticipant>
            {
                new() { UserId = senderUserId },
                new() { UserId = recipientUserId }
            }
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        return await GetConversationAsync(conversation.Id, senderUserId);
    }

    public async Task<List<ConversationDto>> GetMyConversationsAsync(int userId)
    {
        var rows = await _db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User).ThenInclude(u => u.UserDetails)
            .Include(c => c.Messages)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        var result = new List<ConversationDto>();
        foreach (var c in rows)
        {
            var other = c.Participants.FirstOrDefault(p => p.UserId != userId)?.User;
            var last = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unread = c.Messages.Count(m => !m.IsRead && m.SenderId != userId);

            result.Add(new ConversationDto
            {
                Id = c.Id,
                OtherUserId = other?.Id,
                OtherUserName = other?.UserDetails?.FullName ?? other?.Email ?? "User",
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                LastMessage = last?.Content,
                LastMessageAt = last?.SentAt,
                UnreadCount = unread
            });
        }

        return result;
    }

    public async Task<ConversationDetailDto?> GetConversationAsync(int conversationId, int userId)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User).ThenInclude(u => u.UserDetails)
            .Include(c => c.Messages).ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation is null) return null;

        // Membership gate — no admin override by default. Only participants may read.
        if (!conversation.Participants.Any(p => p.UserId == userId)) return null;

        var other = conversation.Participants.FirstOrDefault(p => p.UserId != userId)?.User;

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            OtherUserId = other?.Id,
            OtherUserName = other?.UserDetails?.FullName ?? other?.Email ?? "User",
            Messages = conversation.Messages
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt
                })
                .ToList()
        };
    }
public async Task<MessageDto?> SendMessageAsync(int conversationId, int userId, string content)
    {
        content = content.Trim();

        // Server-side validation is authoritative.
        if (string.IsNullOrEmpty(content)) return null;
        if (content.Length > 2000) return null;

        // Membership is required to send.
        var isMember = await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        if (!isMember) return null;

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);

        var conversation = await _db.Conversations.FindAsync(conversationId);
        if (conversation is not null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var dto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt
        };

        // Deliver to all participants except the sender, route per-user.
        var recipients = await _db.ConversationParticipants
            .Where(p => p.ConversationId == conversationId && p.UserId != userId)
            .Select(p => p.UserId.ToString())
            .ToListAsync();

        if (recipients.Count > 0)
        {
            await _hub.Clients.Users(recipients).SendAsync("MessageReceived", dto);
        }

        return dto;
    }

    public async Task<bool> MarkConversationReadAsync(int conversationId, int userId)
    {
        var isMember = await _db.ConversationParticipants
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        if (!isMember) return false;

        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
        {
            m.IsRead = true;
            m.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _db.Messages.CountAsync(m =>
            m.SenderId != userId &&
            !m.IsRead &&
            _db.ConversationParticipants.Any(p => p.ConversationId == m.ConversationId && p.UserId == userId));

    public async Task<int> GetConversationUnreadCountAsync(int conversationId, int userId)
        => await _db.Messages.CountAsync(m =>
            m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
}