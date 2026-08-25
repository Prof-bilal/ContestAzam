using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class CalendarSyncConfiguration : IEntityTypeConfiguration<CalendarSync>
{
    public void Configure(EntityTypeBuilder<CalendarSync> builder)
    {
        builder.ToTable("CalendarSyncs");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.CalendarType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cs => cs.SyncTimestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(cs => cs.CalendarUrl)
            .HasMaxLength(255);

        builder.HasOne(cs => cs.User)
            .WithMany(u => u.CalendarSyncs)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Event)
            .WithMany(e => e.CalendarSyncs)
            .HasForeignKey(cs => cs.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
