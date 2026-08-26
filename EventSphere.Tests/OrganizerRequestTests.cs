using System.Net;
using System.Net.Http.Json;
using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSphere.Tests;

public class OrganizerRequestTests : IntegrationTestBase
{
    public OrganizerRequestTests(CustomWebApplicationFactory factory) : base(factory) { }

    // ------------------------------------------------------------- Registration as Organizer

    [Fact]
    public async Task Registration_as_Organizer_creates_pending_organizer_request()
    {
        var client = NewClient();
        var email = UniqueEmail("orgreg");

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Org User",
            email,
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Organizer",
            organizationName = "Test Org",
            organizationReason = "We want to organize tech events for the community.",
            organizationExperience = "5 years of event organizing"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        // Should still be Visitor role — not Organizer.
        Assert.Contains("Visitor", auth.User.Roles);
        Assert.DoesNotContain("Organizer", auth.User.Roles);

        // Verify OrganizerRequest was created.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = db.OrganizerRequests.FirstOrDefault(r => r.UserId == auth.User.Id);
        Assert.NotNull(request);
        Assert.Equal(OrganizerRequestStatus.Pending, request!.Status);
        Assert.Equal("Test Org", request.OrganizationName);
    }

    [Fact]
    public async Task Registration_as_Visitor_does_not_create_organizer_request()
    {
        var client = NewClient();
        var email = UniqueEmail("visreg");

        var response = await RegisterAsync(client, email, StrongPassword, "Visitor User");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        Assert.Contains("Visitor", auth.User.Roles);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(db.OrganizerRequests.Any(r => r.UserId == auth.User.Id));
    }

    [Fact]
    public async Task Registration_with_invalid_account_type_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Sneaky",
            email = UniqueEmail(),
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.False(body!.Success);
    }

    [Fact]
    public async Task Registration_with_Participant_account_type_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Sneaky",
            email = UniqueEmail(),
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_registration_without_org_name_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Org User",
            email = UniqueEmail(),
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Organizer"
            // Missing organizationName
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_registration_without_reason_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Org User",
            email = UniqueEmail(),
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Organizer",
            organizationName = "Test Org"
            // Missing organizationReason
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ------------------------------------------------------------- User-facing OrganizerRequest endpoints

    [Fact]
    public async Task Authenticated_user_can_submit_organizer_request()
    {
        var client = NewClient();
        var email = UniqueEmail("orgapp");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        await ConfirmUserEmailAsync(email);
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        var response = await client.PostAsJsonAsync("/api/auth/organizer-requests", new
        {
            organizationName = "My Org",
            reason = "I want to organize community tech events.",
            experience = "Some experience"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_pending_organizer_request_is_rejected()
    {
        var client = NewClient();
        var email = UniqueEmail("duporg");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        await ConfirmUserEmailAsync(email);
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        // First request
        (await client.PostAsJsonAsync("/api/auth/organizer-requests", new
        {
            organizationName = "My Org",
            reason = "I want to organize community tech events."
        })).EnsureSuccessStatusCode();

        // Duplicate
        var second = await client.PostAsJsonAsync("/api/auth/organizer-requests", new
        {
            organizationName = "My Org 2",
            reason = "Another reason for organizing events."
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task User_can_check_their_organizer_request_status()
    {
        var client = NewClient();
        var email = UniqueEmail("orgstat");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();
        await ConfirmUserEmailAsync(email);
        var login = await LoginAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(login);

        var bearer = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
        client.DefaultRequestHeaders.Authorization = bearer;

        // Submit
        (await client.PostAsJsonAsync("/api/auth/organizer-requests", new
        {
            organizationName = "My Org",
            reason = "I want to organize community tech events."
        })).EnsureSuccessStatusCode();

        // Check status
        var response = await client.GetAsync("/api/auth/organizer-requests/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ------------------------------------------------------------- Admin endpoints

    [Fact]
    public async Task Admin_can_list_organizer_requests()
    {
        var adminToken = await CreateUserWithRoleAsync("Admin");
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync("/api/admin/organizer-requests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Non_admin_cannot_access_admin_organizer_endpoints()
    {
        var visitorToken = await CreateUserWithRoleAsync("Visitor");
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", visitorToken);

        var response = await client.GetAsync("/api/admin/organizer-requests");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_approve_organizer_request()
    {
        // Create a user with a pending organizer request.
        var client = NewClient();
        var email = UniqueEmail("orgapprove");
        var reg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Org User",
            email,
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Organizer",
            organizationName = "Test Org",
            organizationReason = "Want to organize events."
        });
        reg.EnsureSuccessStatusCode();
        var auth = await ReadAuthAsync(reg);

        // Get the request ID.
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = db.OrganizerRequests.First(r => r.UserId == auth.User.Id);

        // Admin approves.
        var adminToken = await CreateUserWithRoleAsync("Admin");
        var adminClient = NewClient();
        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var approveResponse = await adminClient.PostAsJsonAsync(
            $"/api/admin/organizer-requests/{request.Id}/approve", new { });
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        // Verify user now has Organizer role.
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.True(await userManager.IsInRoleAsync(user!, AppRoles.Organizer));
    }

    [Fact]
    public async Task Admin_can_reject_organizer_request()
    {
        var client = NewClient();
        var email = UniqueEmail("orgreject");
        var reg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Org User",
            email,
            password = StrongPassword,
            confirmPassword = StrongPassword,
            accountType = "Organizer",
            organizationName = "Test Org",
            organizationReason = "Want to organize events."
        });
        reg.EnsureSuccessStatusCode();
        var auth = await ReadAuthAsync(reg);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = db.OrganizerRequests.First(r => r.UserId == auth.User.Id);

        var adminToken = await CreateUserWithRoleAsync("Admin");
        var adminClient = NewClient();
        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var rejectResponse = await adminClient.PostAsJsonAsync(
            $"/api/admin/organizer-requests/{request.Id}/reject",
            new { rejectionReason = "Not enough experience." });
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

        // Verify user does NOT have Organizer role.
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.False(await userManager.IsInRoleAsync(user!, AppRoles.Organizer));
    }
}
