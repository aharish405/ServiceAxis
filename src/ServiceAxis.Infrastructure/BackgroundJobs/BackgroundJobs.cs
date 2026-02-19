using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ServiceAxis.Infrastructure.BackgroundJobs;

/// <summary>
/// Example recurring background job: platform health check.
/// Runs every minute to verify the platform's critical subsystems are alive.
/// </summary>
public class PlatformHealthCheckJob
{
    private readonly ILogger<PlatformHealthCheckJob> _logger;

    public PlatformHealthCheckJob(ILogger<PlatformHealthCheckJob> logger) =>
        _logger = logger;

    /// <summary>Executes the health check job.</summary>
    public Task ExecuteAsync()
    {
        _logger.LogInformation("[BackgroundJob] PlatformHealthCheckJob executed at {Time}", DateTime.UtcNow);
        // TODO: Add real health checks (DB ping, external API probes, etc.)
        return Task.CompletedTask;
    }
}

/// <summary>
/// Example recurring background job: purges soft-deleted audit logs older than 90 days.
/// Runs daily at midnight.
/// </summary>
public class AuditLogCleanupJob
{
    private readonly ILogger<AuditLogCleanupJob> _logger;

    public AuditLogCleanupJob(ILogger<AuditLogCleanupJob> logger) =>
        _logger = logger;

    /// <summary>Executes the audit log cleanup job.</summary>
    public Task ExecuteAsync()
    {
        _logger.LogInformation("[BackgroundJob] AuditLogCleanupJob executed at {Time}", DateTime.UtcNow);
        // TODO: Query AuditLogs where IsActive = false AND CreatedAt < UtcNow-90d and hard-delete
        return Task.CompletedTask;
    }
}
