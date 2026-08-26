using System.Text.Json;
using EventSphere.Api.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace EventSphere.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a generic 500 ApiResponse. Full details are
/// logged server-side only; the client never sees stack traces, SQL errors, or type names.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var body = ApiResponse.Fail("An unexpected error occurred. Please try again later.");
        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body, SerializerOptions), cancellationToken);

        return true;
    }

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);
}
