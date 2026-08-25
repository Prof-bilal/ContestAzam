using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class EventShareLogConfiguration : IEntityTypeConfiguration<EventShareLog>
{
    public void Configure(EntityTypeBuilder<EventShareLog> builder)
    {
        builder.ToTable("EventShareLogs");

        builder.HasKey(esl => esl.Id);

        builder.Property(esl => esl.Platform)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(esl => esl.ShareTimestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(esl => esl.ShareMessage)
            .HasMaxLength(500);

        builder.HasOne(esl => esl.User)
            .WithMany(u => u.ShareLogs)
            .HasForeignKey(esl => esl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(esl => esl.Event)
            .WithMany(e => e.ShareLogs)
            .HasForeignKey(esl => esl.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
