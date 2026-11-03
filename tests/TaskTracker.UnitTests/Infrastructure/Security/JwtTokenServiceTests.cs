using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Security;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Infrastructure.Security;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    private const string SigningKey = "unit-test-signing-key-at-least-32-bytes";

    private static readonly JwtOptions Options = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = SigningKey,
        LifetimeMinutes = 60
    };

    private readonly JwtTokenService _service = new(Microsoft.Extensions.Options.Options.Create(Options), new FakeTimeProvider(T0));

    // The Id is assigned by the database; set it the way EF does, through the private setter.
    private static User CreateUserWithId(int id)
    {
        var user = new User("Juan Pérez", "user@example.com", "securepassword", new FakePasswordHasher());
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);
        return user;
    }

    [Fact]
    public void CreateAccessToken_With60MinuteLifetime_ReturnsBearerExpiringIn3600Seconds()
    {
        var result = _service.CreateAccessToken(CreateUserWithId(42));

        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
    }

    [Fact]
    public void CreateAccessToken_ForUser_SubjectIsUserId()
    {
        var result = _service.CreateAccessToken(CreateUserWithId(42));

        var token = new JsonWebToken(result.AccessToken);
        Assert.Equal("42", token.Subject);
    }

    [Fact]
    public void CreateAccessToken_AtFixedTime_ExpiresAfterLifetime()
    {
        var result = _service.CreateAccessToken(CreateUserWithId(42));

        var token = new JsonWebToken(result.AccessToken);
        Assert.Equal(new DateTime(2026, 11, 3, 13, 0, 0, DateTimeKind.Utc), token.ValidTo);
    }

    [Fact]
    public async Task CreateAccessToken_ForUser_IsValidForConfiguredIssuerAudienceAndKey()
    {
        var result = _service.CreateAccessToken(CreateUserWithId(42));

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(result.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            // The token was issued at the fake T0, so it would look expired against the real clock.
            ValidateLifetime = false
        });

        Assert.True(validation.IsValid, validation.Exception?.Message);
    }
}
