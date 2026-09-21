namespace TaskTracker.Application.Common.Interfaces;

public interface ICurrentUser
{
    /// The authenticated user's id, or null for anonymous requests (e.g. registration).
    int? UserId { get; }
}