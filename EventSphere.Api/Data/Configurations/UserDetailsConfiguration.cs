using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data.Configurations;

public class UserDetailsConfiguration : IEntityTypeConfiguration<UserDetails>
{
    public void Configure(EntityTypeBuilder<UserDetails> builder)
    {
        builder.ToTable("UserDetails");

        builder.HasKey(ud => ud.Id);

        builder.Property(ud => ud.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ud => ud.Mobile)
            .HasMaxLength(15);

        builder.Property(ud => ud.Department)
            .HasMaxLength(100);

        builder.Property(ud => ud.EnrollmentNo)
            .HasMaxLength(50);

        builder.HasIndex(ud => ud.EnrollmentNo)
            .IsUnique()
            .HasFilter("[EnrollmentNo] IS NOT NULL");

        builder.HasOne(ud => ud.User)
            .WithOne(u => u.UserDetails)
            .HasForeignKey<UserDetails>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
