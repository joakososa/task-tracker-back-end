namespace TaskTracker.Domain.Common;

/// What kind of problem a domain error describes, in business terms. The Api maps each kind to an HTTP status.
public enum ErrorKind
{
    Validation,
    Conflict,
    NotFound,
    Forbidden
}
