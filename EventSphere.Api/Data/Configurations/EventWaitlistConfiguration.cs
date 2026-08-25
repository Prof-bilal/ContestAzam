using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class EventWaitlistConfiguration : IEntityTypeConfiguration<EventWaitlist>
{
    public void Configure(EntityTypeBuilder<EventWaitlist> builder)
    {
        builder.ToTable("EventWaitlists");

        builder.HasKey(ew => ew.Id);

        builder.Property(ew => ew.WaitlistTime)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ew => ew.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(ew => ew.User)
            .WithMany(u => u.WaitlistEntries)
            .HasForeignKey(ew => ew.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ew => ew.Event)
            .WithMany(e => e.WaitlistEntries)
            .HasForeignKey(ew => ew.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ew => new { ew.EventId, ew.UserId })
            .IsUnique();
    }
}
