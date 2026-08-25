using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comments)
            .HasMaxLength(1000);

        builder.Property(f => f.SubmittedOn)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(f => f.Event)
            .WithMany(e => e.Feedbacks)
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Student)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.EventId, f.StudentId })
            .IsUnique();
    }
}
