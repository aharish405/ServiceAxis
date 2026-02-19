using ServiceAxis.Domain.Entities.Sla;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Application contract for the SLA calculation and enforcement engine.
/// </summary>
public interface ISlaService
{
    /// <summary>
    /// Creates and starts an SLA instance for a newly created record.
    /// Looks up the correct SlaPolicy based on table name and priority.
    /// </summary>
    Task<SlaInstance?> StartSlaAsync(
        Guid recordId,
        string tableName,
        int priority,
        CancellationToken ct = default);

    /// <summary>
    /// Pauses the SLA timer (e.g. when state = "Waiting for Customer").
    /// </summary>
    Task PauseSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Resumes a paused SLA timer.
    /// </summary>
    Task ResumeSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Marks the SLA as met (called when record is resolved/closed).
    /// </summary>
    Task CompleteSlaAsync(Guid recordId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates all active SLA instances for breaches and warnings.
    /// Invoked by Hangfire every minute.
    /// </summary>
    Task EvaluateAllActiveAsync(CancellationToken ct = default);
}
