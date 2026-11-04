namespace TaskTracker.Application.Users.UpdateCurrent;

/// Replaces the profile (PUT): a missing avatar removes it. The user comes from the access token.
public record UpdateCurrentUserCommand(string Name, string? Avatar);
