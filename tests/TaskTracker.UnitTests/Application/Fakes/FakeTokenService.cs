using TaskTracker.Application.Auth;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeTokenService : ITokenService
{
    public AccessTokenDto CreateAccessToken(User user) => new($"token-for-{user.Email}", "Bearer", 3600);
}
