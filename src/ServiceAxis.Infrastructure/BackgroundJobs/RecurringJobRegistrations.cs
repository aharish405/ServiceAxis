using Hangfire;

namespace ServiceAxis.Infrastructure.BackgroundJobs;

/// <summary>
/// Registers all platform recurring background jobs with Hangfire.
/// Called from Program.cs after the application has been built.
/// </summary>
public static class RecurringJobRegistrations
{
    public static void Register()
    {
        // ─── Observability ────────────────────────────────────────────────────
        RecurringJob.AddOrUpdate<PlatformHealthCheckJob>(
            "platform-health-check",
            job => job.ExecuteAsync(),
            Cron.Minutely);

        // ─── Data Retention ────────────────────────────────────────────────────
        RecurringJob.AddOrUpdate<AuditLogCleanupJob>(
            "audit-log-cleanup",
            job => job.ExecuteAsync(),
            Cron.Daily);

        RecurringJob.AddOrUpdate<RecordCleanupJob>(
            "record-purge-daily",
            job => job.ExecuteAsync(365),
            Cron.Daily);

        // ─── SLA Engine ────────────────────────────────────────────────────────
        RecurringJob.AddOrUpdate<SlaEvaluationJob>(
            "sla-monitor-job",
            job => job.ExecuteAsync(),
            Cron.Minutely);

        // ─── Notifications ─────────────────────────────────────────────────────
        RecurringJob.AddOrUpdate<NotificationDispatchJob>(
            "notification-dispatch",
            job => job.ExecuteAsync(),
            "*/3 * * * *");   // Every 3 minutes
    }
}
