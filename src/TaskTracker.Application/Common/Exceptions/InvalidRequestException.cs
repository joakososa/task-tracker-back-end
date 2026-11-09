namespace TaskTracker.Application.Common.Exceptions;

/// The request is well formed but cannot be carried out as asked (400), e.g. it points to a user that does not exist.
/// Unlike a ValidationException it has no per-field detail: the code says what is wrong (e.g. USER_NOT_FOUND).
public sealed class InvalidRequestException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
