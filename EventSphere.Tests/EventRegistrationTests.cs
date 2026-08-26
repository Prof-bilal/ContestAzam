using System.Net;
using System.Net.Http.Json;
using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSphere.Tests;

public class EventRegistrationTests : IntegrationTestBase
{
    public EventRegistrationTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Event> CreateApprovedEventAsync(int maxParticipants = 50)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure we have a category and organizer.
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Tech" };
        if (category.Id == 0) db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        var organizerEmail = UniqueEmail("organizer");
        var client = NewClient();
        (await RegisterAsync(client, organizerEmail, StrongPassword, "Event Organizer")).EnsureSuccessStatusCode();
        await Factory.AddUserToRoleAsync(organizerEmail, AppRoles.Organizer);
        var login = await LoginAsync(client, organizerEmail, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var evt = new Event
        {
            Title = "Test Event",
            Description = "A test event",
            CategoryId = category.Id,
            EventDate = DateTime.UtcNow.AddDays(7),
            EventTime = new TimeSpan(10, 0, 0),
            Venue = "Test Venue",
            OrganizerId = auth.User.Id,
            MaxParticipants = maxParticipants,
            Status = EventStatus.Approved
        };

        db.Events.Add(evt);
        await db.SaveChangesAsync();
        return evt;
    }

    [Fact]
    public async Task Visitor_can_register_for_event_and_becomes_Participant()
    {
        var evt = await CreateApprovedEventAsync();
        var client = NewClient();
        var email = UniqueEmail("participant");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        var response = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify Participant role was assigned.
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.True(await userManager.IsInRoleAsync(user!, AppRoles.Participant));
    }

    [Fact]
    public async Task Duplicate_registration_is_blocked()
    {
        var evt = await CreateApprovedEventAsync();
        var client = NewClient();
        var email = UniqueEmail("dupreg");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        // First registration
        (await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { })).EnsureSuccessStatusCode();

        // Second registration
        var second = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Full_event_registration_is_blocked()
    {
        var evt = await CreateApprovedEventAsync(maxParticipants: 1);

        // First user registers.
        var client1 = NewClient();
        var email1 = UniqueEmail("full1");
        (await RegisterAsync(client1, email1, StrongPassword)).EnsureSuccessStatusCode();
        var login1 = await LoginAsync(client1, email1, StrongPassword);
        var auth1 = await ReadAuthAsync(login1);
        client1.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth1.AccessToken);
        (await client1.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { })).EnsureSuccessStatusCode();

        // Second user tries to register.
        var client2 = NewClient();
        var email2 = UniqueEmail("full2");
        (await RegisterAsync(client2, email2, StrongPassword)).EnsureSuccessStatusCode();
        var login2 = await LoginAsync(client2, email2, StrongPassword);
        var auth2 = await ReadAuthAsync(login2);
        client2.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth2.AccessToken);

        var response = await client2.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_register_for_event()
    {
        var evt = await CreateApprovedEventAsync();
        var client = NewClient();

        var response = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registration_failure_does_not_assign_Participant()
    {
        var client = NewClient();
        var email = UniqueEmail("failpart");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        // Try to register for a non-existent event.
        var response = await client.PostAsJsonAsync("/api/events/99999/register", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Verify Participant role was NOT assigned.
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.False(await userManager.IsInRoleAsync(user!, AppRoles.Participant));
    }

    [Fact]
    public async Task Cancelling_registration_does_not_remove_Participant_role()
    {
        var evt = await CreateApprovedEventAsync();
        var client = NewClient();
        var email = UniqueEmail("cancelpart");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        // Register
        (await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { })).EnsureSuccessStatusCode();

        // Verify Participant role
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.True(await userManager.IsInRoleAsync(user!, AppRoles.Participant));

        // Cancel registration
        var cancelResponse = await client.DeleteAsync($"/api/events/{evt.Id}/register");
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // Participant role should still be present.
        user = await userManager.FindByEmailAsync(email);
        Assert.True(await userManager.IsInRoleAsync(user!, AppRoles.Participant));
    }
}
