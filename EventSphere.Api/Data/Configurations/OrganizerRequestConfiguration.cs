using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class OrganizerRequestConfiguration : IEntityTypeConfiguration<OrganizerRequest>
{
    public void Configure(EntityTypeBuilder<OrganizerRequest> builder)
    {
        builder.ToTable("OrganizerRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Experience)
            .HasMaxLength(2000);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UserId FK — Restrict to avoid SQL Server multiple-cascade-path errors.
        // Do not cascade-delete organizer requests when a user is removed.
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ReviewedBy FK — no navigation property, configured as a plain FK.
        // SetNull so the request survives if the reviewing admin is deleted.
        builder.Property<int?>("ReviewedBy")
            .HasColumnName("ReviewedBy");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey("ReviewedBy")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.UserId, r.Status });
    }
}
