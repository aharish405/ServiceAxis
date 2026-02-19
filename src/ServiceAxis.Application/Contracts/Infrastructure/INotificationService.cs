using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Abstraction for the notification dispatch engine.
/// Implementations send via Email, SMS, Teams, etc.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification using a pre-defined template.
    /// </summary>
    Task SendAsync(
        string templateCode,
        IReadOnlyDictionary<string, string> variables,
        IEnumerable<string> recipients,
        NotificationChannelType channel = NotificationChannelType.Email,
        Guid? recordId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Sends an ad-hoc notification without a template.
    /// </summary>
    Task SendDirectAsync(
        string subject,
        string body,
        IEnumerable<string> recipients,
        NotificationChannelType channel = NotificationChannelType.Email,
        CancellationToken ct = default);

    /// <summary>
    /// Processes all pending (queued) notification logs.
    /// Called by Hangfire background job.
    /// </summary>
    Task ProcessPendingAsync(CancellationToken ct = default);
}
