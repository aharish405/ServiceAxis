using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Core engine for processing workflow triggers and managing workflow instances.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Evaluates all active triggers for a given table and event.
    /// If conditions match, starts or advances the associated workflow.
    /// </summary>
    /// <param name="tableId">The table raising the event.</param>
    /// <param name="recordId">The specific record ID.</param>
    /// <param name="eventType">The trigger event type.</param>
    /// <param name="changedField">Required for FieldChanged events.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ProcessTriggersAsync(
        Guid tableId, 
        Guid recordId, 
        WorkflowTriggerEvent eventType, 
        string? changedField = null, 
        CancellationToken ct = default);
}
