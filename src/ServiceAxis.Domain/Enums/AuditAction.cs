namespace ServiceAxis.Domain.Enums;

/// <summary>Audit action categories.</summary>
public enum AuditAction
{
    /// <summary>A new record was created.</summary>
    Create = 1,

    /// <summary>An existing record was updated.</summary>
    Update = 2,

    /// <summary>A record was (soft or hard) deleted.</summary>
    Delete = 3,

    /// <summary>A user authenticated successfully.</summary>
    Login = 4,

    /// <summary>A user logged out.</summary>
    Logout = 5,

    /// <summary>A security-related action occurred.</summary>
    Security = 6,

    /// <summary>A workflow was triggered.</summary>
    WorkflowTriggered = 7,

    /// <summary>Custom/platform-defined action.</summary>
    Custom = 99
}
