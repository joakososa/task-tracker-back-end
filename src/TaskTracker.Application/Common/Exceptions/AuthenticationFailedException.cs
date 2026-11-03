namespace TaskTracker.Application.Common.Exceptions;

/// A login failed. Same error for an unknown email and a wrong password, so accounts cannot be enumerated (ADR 0005).
/// Lives in Application, not Domain: authenticating is a use case, not a rule of any entity.
public sealed class AuthenticationFailedException() : Exception("Invalid email or password.")
{
    public const string Code = "INVALID_CREDENTIALS";
}
