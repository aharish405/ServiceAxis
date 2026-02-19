namespace ServiceAxis.Shared.Wrappers;

/// <summary>
/// Standard API response envelope used across every endpoint.
/// Ensures a consistent JSON contract for all consumers.
/// </summary>
/// <typeparam name="T">The type of the returned data payload.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Gets or sets a value indicating whether the request succeeded.</summary>
    public bool Succeeded { get; set; }

    /// <summary>Gets or sets a human-readable message accompanying the response.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of validation or error messages.</summary>
    public IEnumerable<string> Errors { get; set; } = [];

    /// <summary>Gets or sets the response payload.</summary>
    public T? Data { get; set; }

    /// <summary>Creates a successful response.</summary>
    public static ApiResponse<T> Success(T data, string message = "Operation completed successfully.") =>
        new() { Succeeded = true, Data = data, Message = message };

    /// <summary>Creates a failure response.</summary>
    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}

/// <summary>
/// Non-generic variant of <see cref="ApiResponse{T}"/> for operations that return no data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>Creates a success response with no body.</summary>
    public static ApiResponse SuccessNoData(string message = "Operation completed successfully.") =>
        new() { Succeeded = true, Message = message };

    /// <summary>Creates a failure response with no body.</summary>
    public static new ApiResponse Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Succeeded = false, Message = message, Errors = errors ?? [] };
}
