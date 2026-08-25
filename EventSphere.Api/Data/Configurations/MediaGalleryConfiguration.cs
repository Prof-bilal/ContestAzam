using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class MediaGalleryConfiguration : IEntityTypeConfiguration<MediaGallery>
{
    public void Configure(EntityTypeBuilder<MediaGallery> builder)
    {
        builder.ToTable("MediaGalleries");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FileType)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(m => m.FileUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Caption)
            .HasMaxLength(150);

        builder.Property(m => m.UploadedOn)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(m => m.Event)
            .WithMany(e => e.MediaGalleries)
            .HasForeignKey(m => m.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Uploader)
            .WithMany(u => u.UploadedMedia)
            .HasForeignKey(m => m.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
