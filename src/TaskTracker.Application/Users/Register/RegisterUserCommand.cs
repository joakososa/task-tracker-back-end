namespace TaskTracker.Application.Users.Register;

public record RegisterUserCommand(string Name, string Email, string Password, string? Avatar);