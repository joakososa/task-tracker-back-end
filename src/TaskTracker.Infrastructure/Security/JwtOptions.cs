using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Infrastructure.Security;

/// Settings for issuing access tokens, bound from the "Jwt" configuration section and validated at startup.
/// SigningKey is a secret: user-secrets locally, environment or secret store when deployed (ADR 0005).
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "";

    [Required]
    public string Audience { get; init; } = "";

    // HS256 needs a key of at least 256 bits.
    [Required, MinLength(32)]
    public string SigningKey { get; init; } = "";

    [Range(1, 1440)]
    public int LifetimeMinutes { get; init; }
}
