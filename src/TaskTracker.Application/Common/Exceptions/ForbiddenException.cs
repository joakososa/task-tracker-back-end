namespace TaskTracker.Application.Common.Exceptions;

/// The resource exists but the current user lacks the permission for this use case (403, ADR 0009).
public sealed class ForbiddenException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
