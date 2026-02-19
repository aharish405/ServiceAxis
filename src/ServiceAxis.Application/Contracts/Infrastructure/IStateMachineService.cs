using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Controls the lifecycle state of platform records.
/// All state changes MUST go through this service — never update CurrentStateId directly.
/// </summary>
public interface IStateMachineService
{
    /// <summary>
    /// Returns the set of transitions available for a record in its current state,
    /// filtered by the caller's roles. Only transitions the caller can actually execute are returned.
    /// </summary>
    Task<IReadOnlyList<AvailableTransitionDto>> GetAvailableTransitionsAsync(
        Guid recordId,
        IEnumerable<string> callerRoles,
        CancellationToken ct = default);

    /// <summary>
    /// Validates and executes a state transition. Throws if the transition is not allowed.
    /// Fires an <see cref="ActivityType.StateTransitioned"/> activity entry automatically.
    /// </summary>
    Task<StateChangeResult> ChangeStateAsync(
        Guid recordId,
        Guid targetStateId,
        IEnumerable<string> callerRoles,
        CancellationToken ct = default);

    /// <summary>
    /// Initialises the lifecycle for a newly-created record by finding the initial state
    /// for its table, if one is configured. No-op if the table has no lifecycle defined.
    /// </summary>
    Task InitialiseStateAsync(Guid recordId, Guid tableId, CancellationToken ct = default);

    /// <summary>
    /// Returns all state definitions configured for a table, ordered by <c>Order</c>.
    /// </summary>
    Task<IReadOnlyList<StateDefinitionDto>> GetStatesForTableAsync(Guid tableId, CancellationToken ct = default);
}

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public record AvailableTransitionDto(
    Guid TransitionId,
    Guid ToStateId,
    string ToStateName,
    string ToStateDisplayName,
    string? Label,
    bool RequiresApproval);

public record StateChangeResult(
    Guid RecordId,
    Guid NewStateId,
    string NewStateName,
    string NewStateDisplayName,
    DateTime ChangedAt);

public record StateDefinitionDto(
    Guid Id,
    string StateName,
    string DisplayName,
    bool IsInitialState,
    bool IsFinalState,
    int Order,
    string? Color);
