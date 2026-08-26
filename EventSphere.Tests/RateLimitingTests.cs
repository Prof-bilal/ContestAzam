using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EventSphere.Tests;

/// <summary>
/// Uses a dedicated factory with a deliberately low auth limit so the strict
/// per-endpoint limiter can be exercised. Test parallelization is disabled
/// assembly-wide (see Parallelization.cs) so the env-var override cannot race
/// other factories.
/// </summary>
public class RateLimitingTests
{
    [Fact]
    public async Task Exceeding_the_auth_limit_returns_429_with_retry_after()
    {
        var previous = Environment.GetEnvironmentVariable("RateLimiting__AuthPermitLimit");
        Environment.SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "3");
        try
        {
            await using var factory = new CustomWebApplicationFactory();
            await factory.SeedAsync();
            var client = factory.CreateClient();

            var statuses = new List<HttpStatusCode>();
            HttpResponseMessage? limited = null;

            for (var i = 0; i < 6; i++)
            {
                var response = await client.PostAsJsonAsync("/api/auth/login",
                    new { email = "someone@example.com", password = "whatever" });
                statuses.Add(response.StatusCode);
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    limited ??= response;
            }

            Assert.Contains(HttpStatusCode.TooManyRequests, statuses);
            Assert.NotNull(limited);
            Assert.True(limited!.Headers.Contains("Retry-After"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("RateLimiting__AuthPermitLimit", previous);
        }
    }
}
