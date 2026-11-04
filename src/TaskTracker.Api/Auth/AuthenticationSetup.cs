using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Infrastructure.Security;

namespace TaskTracker.Api.Auth;

public static class AuthenticationSetup
{
    /// Validates the tokens JwtTokenService issues, with the same JwtOptions (ADR 0005).
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    // The default is 5 minutes, meant for clocks drifting across servers; there is one server.
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                // MapInboundClaims stays on (default): "sub" arrives as ClaimTypes.NameIdentifier,
                // which HttpContextCurrentUser reads.
                bearer.Events = new JwtBearerEvents
                {
                    // The default challenge is a 401 with an empty body; the contract wants the Error shape.
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                        await context.Response.WriteAsJsonAsync(new ErrorResponse(
                            NotAuthenticatedException.Code, NotAuthenticatedException.DefaultMessage));
                    }
                };
            });

        // Secure by default: every endpoint needs a token unless it is marked [AllowAnonymous].
        services.AddAuthorization(options => options.FallbackPolicy =
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
