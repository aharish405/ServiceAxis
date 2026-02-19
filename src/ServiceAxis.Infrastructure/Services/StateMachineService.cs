using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

/// <summary>
/// Metadata-driven state machine engine.
/// All lifecycle state changes for platform records flow through this service.
/// </summary>
public class StateMachineService : IStateMachineService
{
    private readonly ServiceAxisDbContext _db;
    private readonly IActivityService    _activity;
    private readonly ISlaService        _sla;
    private readonly ILogger<StateMachineService> _logger;

    public StateMachineService(
        ServiceAxisDbContext db,
        IActivityService activity,
        ISlaService sla,
        ILogger<StateMachineService> logger)
    {
        _db       = db;
        _activity = activity;
        _sla      = sla;
        _logger   = logger;
    }

    // ─── GetAvailableTransitions ───────────────────────────────────────────────

    public async Task<IReadOnlyList<AvailableTransitionDto>> GetAvailableTransitionsAsync(
        Guid recordId,
        IEnumerable<string> callerRoles,
        CancellationToken ct = default)
    {
        var record = await _db.PlatformRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recordId, ct)
            ?? throw new NotFoundException("Record", recordId);

        if (record.CurrentStateId == null)
            return Array.Empty<AvailableTransitionDto>();

        var roleSet = callerRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var transitions = await _db.StateTransitions
            .AsNoTracking()
            .Include(t => t.ToState)
            .Where(t => t.TableId == record.TableId && t.FromStateId == record.CurrentStateId)
            .OrderBy(t => t.ToState.Order)
            .ToListAsync(ct);

        return transitions
            .Where(t => t.AllowedRole == null || roleSet.Contains(t.AllowedRole))
            .Select(t => new AvailableTransitionDto(
                t.Id,
                t.ToStateId,
                t.ToState.StateName,
                t.ToState.DisplayName,
                t.Label,
                t.RequiresApproval))
            .ToList();
    }

    // ─── ChangeState ──────────────────────────────────────────────────────────

    public async Task<StateChangeResult> ChangeStateAsync(
        Guid recordId,
        Guid targetStateId,
        IEnumerable<string> callerRoles,
        CancellationToken ct = default)
    {
        var record = await _db.PlatformRecords
            .FirstOrDefaultAsync(r => r.Id == recordId, ct)
            ?? throw new NotFoundException("Record", recordId);

        var roleSet = callerRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Validate the transition exists and is permitted
        var transition = await _db.StateTransitions
            .Include(t => t.ToState)
            .Include(t => t.FromState)
            .Where(t =>
                t.TableId     == record.TableId &&
                t.FromStateId == record.CurrentStateId &&
                t.ToStateId   == targetStateId)
            .FirstOrDefaultAsync(ct);

        if (transition == null)
        {
            var from = record.CurrentStateId?.ToString() ?? "null";
            throw new BusinessException(
                $"Transition from state '{from}' to '{targetStateId}' is not defined for this table.");
        }

        if (transition.AllowedRole != null && !roleSet.Contains(transition.AllowedRole))
            throw new ForbiddenException(
                $"Your current role does not allow transitioning to '{transition.ToState.DisplayName}'.");

        var fromDisplayName = transition.FromState?.DisplayName ?? record.State;
        var now = DateTime.UtcNow;

        // Apply the state change
        record.CurrentStateId = targetStateId;
        record.State          = transition.ToState.StateName;
        record.StateChangedAt = now;
        record.UpdatedAt      = now;

        // Close/resolve timestamps
        if (transition.ToState.StateName.Equals("resolved", StringComparison.OrdinalIgnoreCase))
        {
            record.ResolvedAt = now;
            await _sla.CompleteSlaAsync(record.Id, ct);
        }

        if (transition.ToState.IsFinalState)
        {
            record.ClosedAt = now;
            await _sla.CompleteSlaAsync(record.Id, ct);
        }

        // Write the activity entry
        await _activity.LogActivityAsync(
            record.TableId,
            record.Id,
            ActivityType.StateTransitioned,
            $"State changed: '{fromDisplayName}' → '{transition.ToState.DisplayName}'",
            isSystem: false,
            changes: new[] { ("state", (Guid?)null, (string?)fromDisplayName, (string?)transition.ToState.DisplayName) },
            ct: ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[StateMachine] Record {RecordId} transitioned {From} → {To}",
            recordId, fromDisplayName, transition.ToState.DisplayName);

        return new StateChangeResult(
            recordId,
            targetStateId,
            transition.ToState.StateName,
            transition.ToState.DisplayName,
            now);
    }

    // ─── InitialiseState ──────────────────────────────────────────────────────

    public async Task InitialiseStateAsync(Guid recordId, Guid tableId, CancellationToken ct = default)
    {
        var initialState = await _db.RecordStateDefinitions
            .AsNoTracking()
            .Where(s => s.TableId == tableId && s.IsInitialState)
            .OrderBy(s => s.Order)
            .FirstOrDefaultAsync(ct);

        if (initialState == null) return; // No lifecycle configured — no-op

        var record = await _db.PlatformRecords.FindAsync(new object[] { recordId }, ct);
        if (record == null) return;

        record.CurrentStateId = initialState.Id;
        record.State          = initialState.StateName;
        record.StateChangedAt = DateTime.UtcNow;
        // No SaveChanges here — caller will commit the outer UoW
    }

    // ─── GetStatesForTable ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<StateDefinitionDto>> GetStatesForTableAsync(
        Guid tableId,
        CancellationToken ct = default)
    {
        var states = await _db.RecordStateDefinitions
            .AsNoTracking()
            .Where(s => s.TableId == tableId && s.IsActive)
            .OrderBy(s => s.Order)
            .ToListAsync(ct);

        return states.Select(s => new StateDefinitionDto(
            s.Id, s.StateName, s.DisplayName,
            s.IsInitialState, s.IsFinalState, s.Order, s.Color)).ToList();
    }
}
