namespace TaskTracker.Application.Common.Exceptions;

/// The request has no valid authenticated user: no token, or a token for a user that no longer exists.
/// The Api writes the same code and message when JwtBearer rejects a token.
public sealed class NotAuthenticatedException() : Exception(DefaultMessage)
{
    public const string Code = "UNAUTHORIZED";
    public const string DefaultMessage = "Missing or invalid access token.";
}
