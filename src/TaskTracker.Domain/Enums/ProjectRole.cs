namespace TaskTracker.Domain.Enums;

/// A user's role inside one project (ADR 0009). Persisted as a string with a CHECK constraint.
public enum ProjectRole
{
    Owner = 0,
    Member = 1
}
