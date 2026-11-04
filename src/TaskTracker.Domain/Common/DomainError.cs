namespace TaskTracker.Domain.Common;

public sealed record DomainError(string Code, ErrorKind Kind);