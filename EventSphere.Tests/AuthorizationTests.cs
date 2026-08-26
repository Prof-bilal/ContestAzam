using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace EventSphere.Tests;

public class AuthorizationTests : IntegrationTestBase
{
    public AuthorizationTests(CustomWebApplicationFactory factory) : base(factory) { }

    private HttpClient AuthedClient(string accessToken)
    {
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    [Fact]
    public async Task Unauthenticated_request_to_protected_endpoint_is_401()
    {
        var response = await NewClient().GetAsync("/api/demo/visitor");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Public_endpoint_allows_anonymous()
    {
        var response = await NewClient().GetAsync("/api/demo/public");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Visitor_cannot_access_participant_area()
    {
        var token = await CreateUserWithRoleAsync("Visitor");
        var response = await AuthedClient(token).GetAsync("/api/demo/participant");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_cannot_access_organizer_area()
    {
        var token = await CreateUserWithRoleAsync("Participant");
        var response = await AuthedClient(token).GetAsync("/api/demo/organizer");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organizer_cannot_access_admin_area()
    {
        var token = await CreateUserWithRoleAsync("Organizer");
        var response = await AuthedClient(token).GetAsync("/api/demo/admin");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_access_admin_area()
    {
        var token = await CreateUserWithRoleAsync("Admin");
        var response = await AuthedClient(token).GetAsync("/api/demo/admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Higher_role_can_access_lower_area()
    {
        var token = await CreateUserWithRoleAsync("Admin");
        var response = await AuthedClient(token).GetAsync("/api/demo/participant");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
