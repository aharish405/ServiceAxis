using Hangfire;

namespace ServiceAxis.Infrastructure.BackgroundJobs;

/// <summary>
/// Contains platform-level recurring background jobs.
/// Jobs are registered during application startup.
/// </summary>
public static class RecurringJobRegistrations
{
    /// <summary>
    /// Registers all platform recurring jobs with Hangfire.
    /// Call this from <c>Program.cs</c> after the application is built.
    /// </summary>
    public static void Register()
    {
        RecurringJob.AddOrUpdate<PlatformHealthCheckJob>(
            "platform-health-check",
            job => job.ExecuteAsync(),
            Cron.Minutely);

        RecurringJob.AddOrUpdate<AuditLogCleanupJob>(
            "audit-log-cleanup",
            job => job.ExecuteAsync(),
            Cron.Daily);
    }
}
