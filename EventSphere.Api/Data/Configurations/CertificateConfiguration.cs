using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.IssuedOn)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.Event)
            .WithMany(e => e.Certificates)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Student)
            .WithMany(u => u.Certificates)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.EventId, c.StudentId })
            .IsUnique();
    }
}
