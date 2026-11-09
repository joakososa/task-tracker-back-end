namespace TaskTracker.Application.Common.Exceptions;

/// The resource a use case looked up does not exist (404). Application, not Domain: entities never look anything up.
/// The code says which resource, e.g. PROJECT_NOT_FOUND; each feature declares its own (see ProjectErrors).
public sealed class NotFoundException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
