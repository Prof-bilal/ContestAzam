using System.ComponentModel.DataAnnotations;
using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMessagingService _messaging;
    private readonly INotificationService _notifications;

    public ConversationsController(IMessagingService messaging, INotificationService notifications)
    {
        _messaging = messaging;
        _notifications = notifications;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>List the authenticated user's conversations (newest updated first).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyConversations()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var conversations = await _messaging.GetMyConversationsAsync(userId.Value);
        return Ok(ApiResponse<List<ConversationDto>>.Ok(conversations));
    }

    /// <summary>Get the unread message count for the authenticated user.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var count = await _messaging.GetUnreadCountAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    /// <summary>Create (or reuse) a 1:1 conversation with another user.</summary>
    [HttpPost]
    [EnableRateLimiting("messaging")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var conversation = await _messaging.GetOrCreateConversationAsync(userId.Value, request.RecipientId);
        if (conversation is null)
            return BadRequest(ApiResponse.Fail("Cannot create a conversation with that user."));

        return Ok(ApiResponse<ConversationDetailDto>.Ok(conversation, "Conversation ready."));
    }

    /// <summary>Get a conversation's detail and messages. Membership is enforced server-side.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var conversation = await _messaging.GetConversationAsync(id, userId.Value);
        if (conversation is null) return NotFound(ApiResponse.Fail("Conversation not found."));

        return Ok(ApiResponse<ConversationDetailDto>.Ok(conversation));
    }

    /// <summary>Send a message. Sender is derived from JWT, never from the body.</summary>
    [HttpPost("{id:int}/messages")]
    [EnableRateLimiting("messaging")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var message = await _messaging.SendMessageAsync(id, userId.Value, request.Content);
        if (message is null)
            return BadRequest(ApiResponse.Fail("Message could not be sent. Check content length and your conversation membership."));

        // Notify the recipient(s) with an in-app notification.
        var conversation = await _messaging.GetConversationAsync(id, userId.Value);
        if (conversation?.OtherUserId is int recipientId)
        {
            await _notifications.SendAsync(
                recipientId,
                Models.NotificationType.MessageReceived,
                "New message",
                "You have a new message.",
                relatedEntityId: conversation.Id,
                relatedEntityType: "Conversation",
                actionUrl: "/messages");
        }

        return Ok(ApiResponse<MessageDto>.Ok(message, "Message sent."));
    }

    /// <summary>Mark all messages in a conversation as read for the current user.</summary>
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkConversationRead(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var marked = await _messaging.MarkConversationReadAsync(id, userId.Value);
        if (!marked) return NotFound(ApiResponse.Fail("Conversation not found."));

        return Ok(ApiResponse.Ok("Conversation marked as read."));
    }
}