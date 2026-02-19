namespace ServiceAxis.Shared.Settings;

/// <summary>
/// Application-level settings shared across all modules.
/// Bind from appsettings.json section "AppSettings".
/// </summary>
public sealed class AppSettings
{
    public const string SectionName = "AppSettings";

    /// <summary>Gets or sets the application display name.</summary>
    public string ApplicationName { get; set; } = "ServiceAxis";

    /// <summary>Gets or sets the current deployment environment name.</summary>
    public string Environment { get; set; } = "Development";

    /// <summary>Gets or sets the base URL of the API (used for link generation).</summary>
    public string BaseUrl { get; set; } = "https://localhost:7001";

    /// <summary>Gets or sets the support email address.</summary>
    public string SupportEmail { get; set; } = "support@serviceaxis.io";

    /// <summary>Gets or sets a value indicating whether detailed exception information is exposed in API responses.</summary>
    public bool ShowDetailedErrors { get; set; }
}
