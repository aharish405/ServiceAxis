using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Sla.Commands;

// ─── Create SLA Definition ────────────────────────────────────────────────────

public record CreateSlaDefinitionCommand(
    string Name,
    string? Description,
    string TableName,
    string Type,
    string ScheduleType,
    int BusinessStartHour,
    int BusinessEndHour,
    string WorkingDaysJson,
    Guid? TenantId) : IRequest<Guid>;

public class CreateSlaDefinitionHandler : IRequestHandler<CreateSlaDefinitionCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateSlaDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateSlaDefinitionCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<SlaType>(cmd.Type, true, out var slaType))
            throw new BusinessException($"Unknown SLA type '{cmd.Type}'.");
        if (!Enum.TryParse<SlaScheduleType>(cmd.ScheduleType, true, out var scheduleType))
            throw new BusinessException($"Unknown schedule type '{cmd.ScheduleType}'.");

        var def = new SlaDefinition
        {
            Name              = cmd.Name,
            Description       = cmd.Description,
            TableName         = cmd.TableName.ToLowerInvariant(),
            Type              = slaType,
            ScheduleType      = scheduleType,
            BusinessStartHour = cmd.BusinessStartHour,
            BusinessEndHour   = cmd.BusinessEndHour,
            WorkingDaysJson   = cmd.WorkingDaysJson,
            TenantId          = cmd.TenantId
        };

        await _uow.Repository<SlaDefinition>().AddAsync(def, ct);
        await _uow.SaveChangesAsync(ct);
        return def.Id;
    }
}

// ─── Add SLA Policy (Priority Tier) ──────────────────────────────────────────

public record AddSlaPolicyCommand(
    Guid SlaDefinitionId,
    string Priority,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    int WarningThresholdPercent,
    bool NotifyOnBreach,
    bool EscalateOnBreach) : IRequest<Guid>;

public class AddSlaPolicyHandler : IRequestHandler<AddSlaPolicyCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public AddSlaPolicyHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(AddSlaPolicyCommand cmd, CancellationToken ct)
    {
        var def = await _uow.Repository<SlaDefinition>().GetByIdAsync(cmd.SlaDefinitionId, ct)
            ?? throw new NotFoundException("SlaDefinition", cmd.SlaDefinitionId);

        if (!Enum.TryParse<SlaPriority>(cmd.Priority, true, out var priority))
            throw new BusinessException($"Unknown priority '{cmd.Priority}'.");

        var conflict = await _uow.Repository<SlaPolicy>()
            .ExistsAsync(p => p.SlaDefinitionId == cmd.SlaDefinitionId && p.Priority == priority && p.IsActive, ct);
        if (conflict)
            throw new ConflictException($"A policy for priority '{cmd.Priority}' already exists in this SLA definition.");

        var policy = new SlaPolicy
        {
            SlaDefinitionId          = cmd.SlaDefinitionId,
            Priority                 = priority,
            ResponseTimeMinutes      = cmd.ResponseTimeMinutes,
            ResolutionTimeMinutes    = cmd.ResolutionTimeMinutes,
            WarningThresholdPercent  = cmd.WarningThresholdPercent,
            NotifyOnBreach           = cmd.NotifyOnBreach,
            EscalateOnBreach         = cmd.EscalateOnBreach
        };

        await _uow.Repository<SlaPolicy>().AddAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);
        return policy.Id;
    }
}

// ─── Update SLA Definition ────────────────────────────────────────────────────

public record UpdateSlaDefinitionCommand(
    Guid Id,
    string Name,
    string? Description,
    int BusinessStartHour,
    int BusinessEndHour,
    bool IsActive) : IRequest;

public class UpdateSlaDefinitionHandler : IRequestHandler<UpdateSlaDefinitionCommand>
{
    private readonly IUnitOfWork _uow;

    public UpdateSlaDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(UpdateSlaDefinitionCommand cmd, CancellationToken ct)
    {
        var def = await _uow.Repository<SlaDefinition>().GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("SlaDefinition", cmd.Id);

        if (def.IsSystemDefinition)
            throw new ForbiddenException("System SLA definitions cannot be modified.");

        def.Name              = cmd.Name;
        def.Description       = cmd.Description;
        def.BusinessStartHour = cmd.BusinessStartHour;
        def.BusinessEndHour   = cmd.BusinessEndHour;
        def.IsActive          = cmd.IsActive;
        def.UpdatedAt         = DateTime.UtcNow;

        _uow.Repository<SlaDefinition>().Update(def);
        await _uow.SaveChangesAsync(ct);
    }
}
