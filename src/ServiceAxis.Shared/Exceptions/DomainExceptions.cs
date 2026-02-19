namespace ServiceAxis.Shared.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// The global exception handler maps this to HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" with key ({key}) was not found.") { }
}

/// <summary>
/// Thrown when a business rule or invariant is violated.
/// The global exception handler maps this to HTTP 422 or 400.
/// </summary>
public sealed class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

/// <summary>
/// Thrown when the caller does not have permission to perform an action.
/// The global exception handler maps this to HTTP 403.
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(message) { }
}

/// <summary>
/// Thrown when a conflict is detected (e.g. duplicate key, concurrent modification).
/// The global exception handler maps this to HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
