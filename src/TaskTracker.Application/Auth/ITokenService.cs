using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Auth;

/// Issues access tokens for an authenticated user. Implemented in Infrastructure (ADR 0005).
public interface ITokenService
{
    AccessTokenDto CreateAccessToken(User user);
}
