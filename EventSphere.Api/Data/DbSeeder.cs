using EventSphere.Api.Common;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace EventSphere.Api.Data;

/// <summary>
/// Deterministic, idempotent seeding of the four application roles and an
/// optional development-only admin account. Safe to run on every startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int>(role));
                if (result.Succeeded)
                    logger.LogInformation("Seeded role {Role}.", role);
                else
                    logger.LogError("Failed seeding role {Role}: {Errors}", role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        await SeedDevAdminAsync(services, config, logger);
    }

    /// <summary>
    /// Creates an initial Admin account ONLY when explicitly enabled and provided
    /// via configuration (env vars / user secrets). No credentials are hardcoded.
    /// </summary>
    private static async Task SeedDevAdminAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var section = config.GetSection("SeedAdmin");
        if (!section.GetValue<bool>("Enabled")) return;

        var email = section["Email"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SeedAdmin is enabled but Email/Password are not configured; skipping.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        if (await userManager.FindByEmailAsync(email) is not null)
            return; // idempotent

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Role = UserRole.Admin
        };

        var created = await userManager.CreateAsync(admin, password);
        if (!created.Succeeded)
        {
            logger.LogError("Failed creating seed admin: {Errors}",
                string.Join(", ", created.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, AppRoles.Admin);

        db.UserDetails.Add(new UserDetails { UserId = admin.Id, FullName = "Administrator" });
        await db.SaveChangesAsync();

        logger.LogWarning("Seeded development admin account {Email}. Rotate this credential outside development.", email);
    }
}
