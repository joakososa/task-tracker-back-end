namespace TaskTracker.Application.Auth;

/// The contract's LoginResponse. ExpiresIn is in seconds.
public sealed record AccessTokenDto(string AccessToken, string TokenType, int ExpiresIn);
