namespace ServiceAxis.Shared.Settings;

/// <summary>
/// Strongly-typed settings for JWT token generation and validation.
/// Bind from appsettings.json section "JwtSettings".
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>Gets or sets the issuer (iss claim).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Gets or sets the audience (aud claim).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Gets or sets the HMAC-SHA256 signing key. Must be at least 32 characters.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the access token expiry in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>Gets or sets the refresh token expiry in days.</summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
