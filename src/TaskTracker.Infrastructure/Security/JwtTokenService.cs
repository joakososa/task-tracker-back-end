using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Application.Auth;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Security;

/// Issues HS256-signed JWTs (ADR 0005).
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _clock;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenDto CreateAccessToken(User user)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var lifetime = TimeSpan.FromMinutes(_options.LifetimeMinutes);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + lifetime,
            // JwtBearer maps "sub" to ClaimTypes.NameIdentifier on the way in, which HttpContextCurrentUser reads.
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(CultureInfo.InvariantCulture),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            },
            SigningCredentials = _credentials
        });

        return new AccessTokenDto(token, "Bearer", (int)lifetime.TotalSeconds);
    }
}
