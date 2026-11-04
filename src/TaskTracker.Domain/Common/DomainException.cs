namespace TaskTracker.Domain.Common;

public class DomainException(DomainError error, string message) : Exception(message)
{
    public string Code { get; } = error.Code;
    public ErrorKind Kind { get; } = error.Kind;
}
