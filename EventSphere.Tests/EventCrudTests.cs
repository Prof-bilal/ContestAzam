using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSphere.Tests;

public class EventCrudTests : IntegrationTestBase
{
    public EventCrudTests(CustomWebApplicationFactory factory) : base(factory) { }

    private static System.Net.Http.Headers.AuthenticationHeaderValue AuthHeader(string token) =>
        new("Bearer", token);

    private async Task<(string OrganizerToken, int OrganizerId, int CategoryId)> SetupOrganizerWithCategoryAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Tech" };
        if (category.Id == 0) db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        var email = UniqueEmail("org");
        var client = NewClient();
        (await RegisterAsync(client, email, StrongPassword, "Organizer")).EnsureSuccessStatusCode();
        await Factory.AddUserToRoleAsync(email, AppRoles.Organizer);
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        return (auth.AccessToken, auth.User.Id, category.Id);
    }

    // ───────────────────────────── EVENT CREATION ─────────────────────────────

    [Fact]
    public async Task TC_EVENT_001_Organizer_creates_valid_event()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "New Event",
            description = "A great event",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            venue = "Main Hall",
            maxParticipants = 100
        });

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>();
        Assert.True(body!.Success);
        Assert.Equal("New Event", body.Data!.Title);
    }

    [Fact]
    public async Task TC_EVENT_002_Visitor_cannot_create_event()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        // Register as visitor (not organizer)
        var visitorEmail = UniqueEmail("vis");
        (await RegisterAsync(client, visitorEmail, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, visitorEmail, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Hack",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 10
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TC_EVENT_005_Unauthenticated_user_cannot_create_event()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Anon Event",
            eventDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 10
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TC_EVENT_006_Missing_title_returns_400()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 10
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC_EVENT_008_Invalid_capacity_returns_400()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Bad Capacity",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Event_created_as_draft_does_not_appear_in_public_listing()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Draft Event",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50,
            saveAsDraft = true
        });
        var created = await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>();
        Assert.Equal("Draft", created!.Data!.Status);

        // Public listing should not show it
        var publicClient = NewClient();
        var listJson = await publicClient.GetFromJsonAsync<JsonElement>("/api/events");
        var events = listJson.GetProperty("data").GetProperty("events");
        var hasEvent = events.EnumerateArray().Any(e => e.GetProperty("id").GetInt32() == created.Data.Id);
        Assert.False(hasEvent, "Draft event should not appear in public listing");
    }

    // ───────────────────────────── OWNERSHIP ─────────────────────────────

    [Fact]
    public async Task TC_OWN_002_Organizer_B_cannot_edit_Organizer_A_event()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        // Create event as Organizer A
        var (tokenA, idA, _) = await SetupOrganizerWithCategoryAsync();
        var clientA = NewClient();
        clientA.DefaultRequestHeaders.Authorization = AuthHeader(tokenA);
        var create = await clientA.PostAsJsonAsync("/api/events", new
        {
            title = "Org A Event",
            categoryId = category.Id,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        // Create Organizer B
        var (tokenB, _, _) = await SetupOrganizerWithCategoryAsync();
        var clientB = NewClient();
        clientB.DefaultRequestHeaders.Authorization = AuthHeader(tokenB);

        // Organizer B tries to edit Organizer A's event
        var response = await clientB.PutAsJsonAsync($"/api/events/{evt.Id}", new
        {
            title = "Hijacked",
            categoryId = category.Id,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC_OWN_003_Organizer_B_cannot_delete_Organizer_A_event()
    {
        var (tokenA, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var clientA = NewClient();
        clientA.DefaultRequestHeaders.Authorization = AuthHeader(tokenA);
        var create = await clientA.PostAsJsonAsync("/api/events", new
        {
            title = "Delete Me",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50,
            saveAsDraft = true
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        var (tokenB, _, _) = await SetupOrganizerWithCategoryAsync();
        var clientB = NewClient();
        clientB.DefaultRequestHeaders.Authorization = AuthHeader(tokenB);

        var response = await clientB.DeleteAsync($"/api/events/{evt.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ───────────────────────────── STATUS TRANSITIONS ─────────────────────────────

    [Fact]
    public async Task Draft_event_can_be_published()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Publish Me",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50,
            saveAsDraft = true
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;
        Assert.Equal("Draft", evt.Status);

        var publish = await client.PatchAsync($"/api/events/{evt.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        var updated = await client.GetFromJsonAsync<ApiResponse<EventSummaryDto>>($"/api/events/{evt.Id}");
        Assert.Equal("PendingApproval", updated!.Data!.Status);
    }

    [Fact]
    public async Task Approved_event_can_be_cancelled()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        // Create and manually approve via DB
        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Cancel Me",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        // Manually set to Approved since PendingApproval requires admin approval
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbEvt = await db.Events.FindAsync(evt.Id);
        dbEvt!.Status = EventStatus.Approved;
        await db.SaveChangesAsync();

        var cancel = await client.PatchAsync($"/api/events/{evt.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var updated = await client.GetFromJsonAsync<ApiResponse<EventSummaryDto>>($"/api/events/{evt.Id}");
        Assert.Equal("Cancelled", updated!.Data!.Status);
    }

    [Fact]
    public async Task Cancelled_event_blocks_registration()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Cancelled Event",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        // Cancel
        await client.PatchAsync($"/api/events/{evt.Id}/cancel", null);

        // Try to register
        var regClient = NewClient();
        var email = UniqueEmail("regaftercancel");
        (await RegisterAsync(regClient, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(regClient, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        regClient.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        var regResponse = await regClient.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.BadRequest, regResponse.StatusCode);
    }

    // ───────────────────────────── EVENT DISCOVERY ─────────────────────────────

    [Fact]
    public async Task Public_events_only_show_approved()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        // Create draft
        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Draft Only",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50,
            saveAsDraft = true
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        var publicClient = NewClient();
        var listJson = await publicClient.GetFromJsonAsync<JsonElement>("/api/events");
        var events = listJson.GetProperty("data").GetProperty("events");
        var hasEvent = events.EnumerateArray().Any(e => e.GetProperty("id").GetInt32() == evt.Id);
        Assert.False(hasEvent, "Draft event should not appear in public listing");
    }

    [Fact]
    public async Task Pagination_works()
    {
        // Create approved events directly via DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        var organizerId = db.Users.First().Id;

        for (int i = 0; i < 3; i++)
        {
            db.Events.Add(new Event
            {
                Title = $"Page Event {i}",
                CategoryId = category.Id,
                EventDate = DateTime.UtcNow.AddDays(10 + i),
                EventTime = TimeSpan.FromHours(10),
                MaxParticipants = 50,
                OrganizerId = organizerId,
                Status = EventStatus.Approved
            });
        }
        await db.SaveChangesAsync();

        var publicClient = NewClient();
        var listJson = await publicClient.GetFromJsonAsync<JsonElement>("/api/events?page=1&pageSize=2");
        var events = listJson.GetProperty("data").GetProperty("events");
        Assert.Equal(2, events.GetArrayLength());
        var totalPages = listJson.GetProperty("data").GetProperty("totalPages").GetInt32();
        Assert.True(totalPages >= 2);
    }

    [Fact]
    public async Task Search_filters_events()
    {
        // Create approved event directly via DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        var organizerId = db.Users.First().Id;

        db.Events.Add(new Event
        {
            Title = "Python Workshop",
            CategoryId = category.Id,
            EventDate = DateTime.UtcNow.AddDays(10),
            EventTime = TimeSpan.FromHours(10),
            MaxParticipants = 50,
            OrganizerId = organizerId,
            Status = EventStatus.Approved
        });
        await db.SaveChangesAsync();

        var publicClient = NewClient();
        var resultsJson = await publicClient.GetFromJsonAsync<JsonElement>("/api/events?search=Python");
        var events = resultsJson.GetProperty("data").GetProperty("events");
        Assert.Equal(1, events.GetArrayLength());
        Assert.Contains("Python", events[0].GetProperty("title").GetString()!);
    }

    [Fact]
    public async Task PageSize_is_clamped_to_50()
    {
        var publicClient = NewClient();
        var response = await publicClient.GetAsync("/api/events?pageSize=999");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("success").GetBoolean());
    }

    // ───────────────────────────── REGISTRATION ─────────────────────────────

    [Fact]
    public async Task TC_REG_002_Duplicate_registration_blocked()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        var evt = new Event
        {
            Title = "Dup Test",
            CategoryId = category.Id,
            EventDate = DateTime.UtcNow.AddDays(7),
            EventTime = TimeSpan.FromHours(10),
            MaxParticipants = 50,
            Status = EventStatus.Approved,
            OrganizerId = db.Users.First().Id
        };
        db.Events.Add(evt);
        await db.SaveChangesAsync();

        var client = NewClient();
        var email = UniqueEmail("dupreg");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        // Register
        var r1 = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);

        // Duplicate
        var r2 = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.Conflict, r2.StatusCode);
    }

    [Fact]
    public async Task TC_REG_008_Failed_registration_does_not_assign_Participant()
    {
        var client = NewClient();
        var email = UniqueEmail("failreg");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        // Register for non-existent event
        var response = await client.PostAsJsonAsync("/api/events/99999/register", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Should NOT have Participant role
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.False(await userManager.IsInRoleAsync(user!, AppRoles.Participant));
    }

    [Fact]
    public async Task Successful_registration_assigns_Participant_role()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        var evt = new Event
        {
            Title = "Role Test",
            CategoryId = category.Id,
            EventDate = DateTime.UtcNow.AddDays(7),
            EventTime = TimeSpan.FromHours(10),
            MaxParticipants = 50,
            Status = EventStatus.Approved,
            OrganizerId = db.Users.First().Id
        };
        db.Events.Add(evt);
        await db.SaveChangesAsync();

        var client = NewClient();
        var email = UniqueEmail("rolepromote");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        var response = await client.PostAsJsonAsync($"/api/events/{evt.Id}/register", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope2 = Factory.Services.CreateScope();
        var userManager = scope2.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.True(await userManager.IsInRoleAsync(user!, AppRoles.Participant));
    }

    // ───────────────────────────── CATEGORIES ─────────────────────────────

    [Fact]
    public async Task Organizer_can_create_category()
    {
        var (token, _, _) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var response = await client.PostAsJsonAsync("/api/organizer/categories", new
        {
            name = "New Category",
            description = "A test category"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_can_update_category()
    {
        var (token, _, _) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/organizer/categories", new
        {
            name = "To Update"
        });
        var cat = (await create.Content.ReadFromJsonAsync<ApiResponse<EventCategoryDto>>())!.Data!;

        var update = await client.PutAsJsonAsync($"/api/organizer/categories/{cat.Id}", new
        {
            name = "Updated Name"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
    }

    [Fact]
    public async Task Category_with_events_cannot_be_deleted()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        // Create an event in this category
        await client.PostAsJsonAsync("/api/events", new
        {
            title = "Cat Test",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });

        var delete = await client.DeleteAsync($"/api/organizer/categories/{categoryId}");
        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
    }

    [Fact]
    public async Task Visitor_cannot_create_category()
    {
        var client = NewClient();
        var email = UniqueEmail("viscat");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/organizer/categories", new
        {
            name = "Hacked Category"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ───────────────────────────── ADMIN EVENT MANAGEMENT ─────────────────────────────

    [Fact]
    public async Task Admin_can_approve_pending_event()
    {
        var (token, _, categoryId) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Approve Me",
            categoryId,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        var adminToken = await CreateUserWithRoleAsync("Admin");
        var adminClient = NewClient();
        adminClient.DefaultRequestHeaders.Authorization = AuthHeader(adminToken);

        var approve = await adminClient.PatchAsync($"/api/admin/events/{evt.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
    }

    [Fact]
    public async Task Visitor_cannot_access_admin_event_endpoints()
    {
        var client = NewClient();
        var email = UniqueEmail("visadmin");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);
        client.DefaultRequestHeaders.Authorization = AuthHeader(auth.AccessToken);

        var response = await client.GetAsync("/api/admin/events");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_cannot_access_admin_event_endpoints()
    {
        var (token, _, _) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var response = await client.GetAsync("/api/admin/events");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ───────────────────────────── REVIEWS ─────────────────────────────

    [Fact]
    public async Task Unauthenticated_user_can_read_reviews()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        var evt = new Event
        {
            Title = "Review Test",
            CategoryId = category.Id,
            EventDate = DateTime.UtcNow.AddDays(-1),
            EventTime = TimeSpan.FromHours(10),
            MaxParticipants = 50,
            Status = EventStatus.Approved,
            OrganizerId = db.Users.First().Id
        };
        db.Events.Add(evt);
        await db.SaveChangesAsync();

        var client = NewClient();
        var response = await client.GetAsync($"/api/events/{evt.Id}/reviews");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ───────────────────────────── ATTENDEE MANAGEMENT ─────────────────────────────

    [Fact]
    public async Task Organizer_can_view_own_event_attendees()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        var (token, organizerId, _) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var create = await client.PostAsJsonAsync("/api/events", new
        {
            title = "Attendee Test",
            categoryId = category.Id,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        var response = await client.GetAsync($"/api/organizer/events/{evt.Id}/attendees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_cannot_view_other_organizer_attendees()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = db.EventCategories.FirstOrDefault() ?? new EventCategory { Name = "Test" };
        if (category.Id == 0) db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        // Create event as Org A
        var (tokenA, _, _) = await SetupOrganizerWithCategoryAsync();
        var clientA = NewClient();
        clientA.DefaultRequestHeaders.Authorization = AuthHeader(tokenA);
        var create = await clientA.PostAsJsonAsync("/api/events", new
        {
            title = "Private Event",
            categoryId = category.Id,
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });
        var evt = (await create.Content.ReadFromJsonAsync<ApiResponse<EventSummaryDto>>())!.Data!;

        // Org B tries to access attendees
        var (tokenB, _, _) = await SetupOrganizerWithCategoryAsync();
        var clientB = NewClient();
        clientB.DefaultRequestHeaders.Authorization = AuthHeader(tokenB);

        var response = await clientB.GetAsync($"/api/organizer/events/{evt.Id}/attendees");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ───────────────────────────── IMAGE UPLOAD ─────────────────────────────

    [Fact]
    public async Task Image_upload_rejects_non_image_content_type()
    {
        var (token, _, _) = await SetupOrganizerWithCategoryAsync();
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = AuthHeader(token);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "test.pdf");

        var response = await client.PostAsync("/api/organizer/upload-image", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ───────────────────────────── EDGE CASES ─────────────────────────────

    [Fact]
    public async Task Nonexistent_event_returns_404()
    {
        var client = NewClient();
        var response = await client.GetAsync("/api/events/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Categories_endpoint_is_public()
    {
        var client = NewClient();
        var response = await client.GetAsync("/api/events/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_stats_only_includes_own_events()
    {
        var (tokenA, _, _) = await SetupOrganizerWithCategoryAsync();
        var (tokenB, _, _) = await SetupOrganizerWithCategoryAsync();

        // Org A creates an event
        var clientA = NewClient();
        clientA.DefaultRequestHeaders.Authorization = AuthHeader(tokenA);
        await clientA.PostAsJsonAsync("/api/events", new
        {
            title = "OrgA Event",
            eventDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
            eventTime = "10:00:00",
            maxParticipants = 50
        });

        // Check Org A stats
        var statsA = await clientA.GetFromJsonAsync<ApiResponse<OrganizerEventStatsDto>>("/api/organizer/events/stats");
        Assert.Equal(1, statsA!.Data!.TotalEvents);

        // Check Org B stats - should be 0
        var clientB = NewClient();
        clientB.DefaultRequestHeaders.Authorization = AuthHeader(tokenB);
        var statsB = await clientB.GetFromJsonAsync<ApiResponse<OrganizerEventStatsDto>>("/api/organizer/events/stats");
        Assert.Equal(0, statsB!.Data!.TotalEvents);
    }
}
