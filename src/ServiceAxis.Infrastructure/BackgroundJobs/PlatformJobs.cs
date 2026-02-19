using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.BackgroundJobs;

/// <summary>
/// Recurring job that evaluates all active SLA instances and fires warnings/breaches.
/// Scheduled every 5 minutes via Hangfire.
/// </summary>
public class SlaEvaluationJob
{
    private readonly ISlaService _slaService;
    private readonly ILogger<SlaEvaluationJob> _logger;

    public SlaEvaluationJob(ISlaService slaService, ILogger<SlaEvaluationJob> logger)
    {
        _slaService = slaService;
        _logger     = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("[SlaEvaluationJob] Starting SLA evaluation at {Time}", DateTime.UtcNow);
        try
        {
            await _slaService.EvaluateAllActiveAsync();
            _logger.LogInformation("[SlaEvaluationJob] Completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SlaEvaluationJob] Failed during SLA evaluation.");
        }
    }
}

/// <summary>
/// Recurring job that retries any PENDING or FAILED notification log entries.
/// Scheduled every 3 minutes via Hangfire.
/// </summary>
public class NotificationDispatchJob
{
    private readonly INotificationService _notifications;
    private readonly ILogger<NotificationDispatchJob> _logger;

    public NotificationDispatchJob(INotificationService notifications, ILogger<NotificationDispatchJob> logger)
    {
        _notifications = notifications;
        _logger        = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("[NotificationDispatchJob] Processing pending notifications at {Time}", DateTime.UtcNow);
        try
        {
            await _notifications.ProcessPendingAsync();
            _logger.LogInformation("[NotificationDispatchJob] Completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationDispatchJob] Failed during notification dispatch.");
        }
    }
}

/// <summary>
/// Daily job that hard-deletes soft-deleted PlatformRecords older than the configured retention window.
/// Default: 365 days. Runs once per day.
/// </summary>
public class RecordCleanupJob
{
    private readonly ServiceAxisDbContext _db;
    private readonly ILogger<RecordCleanupJob> _logger;

    public RecordCleanupJob(ServiceAxisDbContext db, ILogger<RecordCleanupJob> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task ExecuteAsync(int retentionDays = 365)
    {
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        _logger.LogInformation("[RecordCleanupJob] Purging deleted records older than {Cutoff}", cutoff);

        try
        {
            var count = await _db.PlatformRecords
                .Where(r => r.IsDeleted && r.UpdatedAt < cutoff)
                .ExecuteDeleteAsync();

            _logger.LogInformation("[RecordCleanupJob] Purged {Count} deleted records.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RecordCleanupJob] Failed during record cleanup.");
        }
    }
}
