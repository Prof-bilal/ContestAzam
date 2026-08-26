namespace EventSphere.Api.Common;

/// <summary>
/// Consistent envelope for all auth API responses. Error bodies never contain
/// stack traces, SQL details, or internal type names.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; init; }

    public static ApiResponse Ok(string message) => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, IDictionary<string, string[]>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse Fail(string message, string field, string error) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, string[]> { [field] = new[] { error } }
        };
}

/// <summary>Envelope carrying a typed payload on success.</summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "OK") =>
        new() { Success = true, Message = message, Data = data };
}
