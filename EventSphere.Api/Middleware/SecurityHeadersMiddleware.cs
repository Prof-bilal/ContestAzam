namespace EventSphere.Api.Middleware;

/// <summary>Adds conservative security response headers to every response.</summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        // API returns JSON only; a strict CSP prevents any content from being framed/executed.
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        await _next(context);
    }
}
