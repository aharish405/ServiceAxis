namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Application contract for the record assignment engine.
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Auto-assigns a record to the best available group member
    /// based on the queue's assignment strategy.
    /// </summary>
    Task<string?> AutoAssignAsync(
        Guid recordId,
        string tableName,
        int priority,
        CancellationToken ct = default);

    /// <summary>
    /// Manually assigns a record to a specific user and/or group.
    /// </summary>
    Task AssignAsync(
        Guid recordId,
        string? userId,
        Guid? groupId,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves the best queue for a record based on routing conditions.
    /// </summary>
    Task<Guid?> ResolveQueueAsync(
        string tableName,
        int priority,
        CancellationToken ct = default);
}
