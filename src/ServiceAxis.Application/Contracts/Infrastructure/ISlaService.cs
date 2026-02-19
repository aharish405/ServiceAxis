using ServiceAxis.Domain.Entities.Sla;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Application contract for the SLA calculation and enforcement engine.
/// </summary>
public interface ISlaService
{
    /// <summary>
    /// Starts the SLA timer for a newly created or updated record.
    /// Checks if any SLA definition matches the record's criteria (e.g. Priority).
    /// </summary>
    Task StartSlaAsync(Guid recordId, string tableName, int priority, Guid? tenantId, CancellationToken ct = default);

    /// <summary>
    /// Pauses any active Response/Resolution SLA timers for the record.
    /// </summary>
    Task PauseSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Resumes paused SLA timers for the record.
    /// </summary>
    Task ResumeSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Stops the 'First Response' SLA timer if active.
    /// </summary>
    Task MarkResponseCompletedAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Stops the 'Resolution' SLA timer (and effectively completes the lifecycle).
    /// </summary>
    Task CompleteSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates all active SLA instances for breaches and triggers escalations.
    /// Invoked by Hangfire recurring job.
    /// </summary>
    Task EvaluateAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves the current status of all SLA timers for a record.
    /// </summary>
    Task<List<SlaStatusDto>> GetRecordSlaStatusAsync(Guid recordId, CancellationToken ct = default);
}

public record SlaStatusDto(
    Guid SlaInstanceId,
    string Metric,
    DateTime TargetTime,
    int RemainingMinutes,
    bool IsBreached,
    bool IsPaused,
    string Status);
