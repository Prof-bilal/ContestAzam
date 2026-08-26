using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EventSphere.Tests;

/// <summary>
/// Boots the real API in the "Testing" environment with an isolated in-memory
/// database. Config that Program.cs reads eagerly (JWT key, rate limits, origins)
/// is injected via environment variables, which are guaranteed to be present at
/// builder-creation time (unlike ConfigureAppConfiguration under WAF).
/// Each factory instance owns its own rate-limiter state, isolating limits per test class.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "INTEGRATION_TEST_SIGNING_KEY_AT_LEAST_32_BYTES_LONG";

    private readonly string _dbName = "EventSphereTests-" + Guid.NewGuid();

    static CustomWebApplicationFactory()
    {
        // Ordering-safe injection for eager config reads in Program.cs.
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "EventSphere");
        Environment.SetEnvironmentVariable("Jwt__Audience", "EventSphere");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15");
        Environment.SetEnvironmentVariable("Frontend__AllowedOrigins__0", "http://localhost:5173");
        Environment.SetEnvironmentVariable("RefreshToken__CookieSameSite", "Lax");
        // High by default so functional tests never trip the limiter; the rate-limit
        // test overrides RateLimiting__AuthPermitLimit for its own dedicated factory.
        Environment.SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "100000");
        Environment.SetEnvironmentVariable("RateLimiting__AuthWindowSeconds", "60");
        Environment.SetEnvironmentVariable("RateLimiting__EmailPermitLimit", "100000");
        Environment.SetEnvironmentVariable("RateLimiting__EmailWindowSeconds", "60");
        Environment.SetEnvironmentVariable("RateLimiting__GeneralPermitLimit", "100000");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // Override Identity options for testing: disable email confirmation so
            // existing tests continue to pass. Dedicated email-verification tests
            // confirm the user manually via UserManager.
            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            });
        });
    }

    /// <summary>Ensures the schema exists and the four roles are seeded. Idempotent.</summary>
    public async Task SeedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        foreach (var role in AppRoles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
    }

    /// <summary>Adds an existing user (by email) to a role — used to set up authorization scenarios.</summary>
    public async Task AddUserToRoleAsync(string email, string role)
    {
        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.FindByEmailAsync(email);
        await users.AddToRoleAsync(user!, role);
    }
}
