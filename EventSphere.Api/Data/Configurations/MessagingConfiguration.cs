using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventSphere.Api.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.Conversation)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");
        builder.HasKey(p => new { p.ConversationId, p.UserId });

        // Restrict user deletion so an unrelated user removal never cascades and
        // silently wipes communication history (avoids a second cascade path).
        builder.HasOne(p => p.User)
            .WithMany(u => u.ConversationParticipants)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enforce that a user can appear only once per conversation.
        builder.HasIndex(p => new { p.ConversationId, p.UserId }).IsUnique();
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.SentAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.IsRead)
            .HasDefaultValue(false);

        // Sender deletion is restricted: history is preserved even if a user is removed.
        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cover read-ordering and unread scans.
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });
        builder.HasIndex(m => new { m.SenderId, m.IsRead });
    }
}