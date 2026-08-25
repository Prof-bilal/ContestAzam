using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class EventSeatingConfiguration : IEntityTypeConfiguration<EventSeating>
{
    public void Configure(EntityTypeBuilder<EventSeating> builder)
    {
        builder.ToTable("EventSeatings");

        builder.HasKey(es => es.EventId);

        builder.Property(es => es.TotalSeats)
            .IsRequired();

        builder.Property(es => es.SeatsBooked)
            .HasDefaultValue(0);

        builder.Property(es => es.WaitlistEnabled)
            .HasDefaultValue(false);

        builder.HasOne(es => es.Event)
            .WithOne(e => e.EventSeating)
            .HasForeignKey<EventSeating>(es => es.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Venue)
            .WithOne(v => v.EventSeating)
            .HasForeignKey<EventSeating>(es => es.VenueId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
