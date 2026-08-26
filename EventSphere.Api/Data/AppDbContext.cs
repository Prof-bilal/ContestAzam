using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventSphere.Api.Models;

namespace EventSphere.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserDetails> UserDetails => Set<UserDetails>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<MediaGallery> MediaGalleries => Set<MediaGallery>();
    public DbSet<EventSeating> EventSeatings => Set<EventSeating>();
    public DbSet<EventWaitlist> EventWaitlists => Set<EventWaitlist>();
    public DbSet<CalendarSync> CalendarSyncs => Set<CalendarSync>();
    public DbSet<EventShareLog> EventShareLogs => Set<EventShareLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OrganizerRequest> OrganizerRequests => Set<OrganizerRequest>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables
        modelBuilder.Entity<AppUser>(e => e.ToTable("Users"));
        modelBuilder.Entity<IdentityRole<int>>(e => e.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserRole<int>>(e => e.ToTable("UserRoles"));
        modelBuilder.Entity<IdentityUserClaim<int>>(e => e.ToTable("UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<int>>(e => e.ToTable("UserLogins"));
        modelBuilder.Entity<IdentityUserToken<int>>(e => e.ToTable("UserTokens"));
        modelBuilder.Entity<IdentityRoleClaim<int>>(e => e.ToTable("RoleClaims"));

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
