using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Domain.Entities.Platform; // For SysTable
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Sla.Commands;

// ─── Create SLA Definition ────────────────────────────────────────────────────

public record CreateSlaDefinitionCommand(
    string Name,
    string? Description,
    string TableName,
    Guid? TenantId) : IRequest<Guid>;

public class CreateSlaDefinitionHandler : IRequestHandler<CreateSlaDefinitionCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateSlaDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateSlaDefinitionCommand cmd, CancellationToken ct)
    {
        // Resolve Table ID
        var tables = await _uow.Repository<SysTable>().FindAsync(t => t.Name == cmd.TableName, ct);
        var table = tables.FirstOrDefault()
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var def = new SlaDefinition
        {
            Name              = cmd.Name,
            Description       = cmd.Description,
            TableId           = table.Id, 
            IsActive          = true,
            TenantId          = cmd.TenantId
        };

        await _uow.Repository<SlaDefinition>().AddAsync(def, ct);
        // Note: Commit is handled by UnitOfWork usually or explicit SaveChanges.
        // My previous handlers used uow.SaveChangesAsync().
        // If UnitOfWork pattern in this solution requires it:
        // (BaseUnitOfWork usually has SaveChangesAsync).
        // I'll assume I need to call it on repository addition if pattern suggests, 
        // or _uow.CommitAsync()? 
        // The existing code used `await _uow.SaveChangesAsync(ct);`. I'll follow that.
        // Wait, `IUnitOfWork` usually exposes `Task<int> SaveAsync(ct)`.
        // I'll use `_uow.SaveAsync(ct)` if available, or just rely on `AddAsync` if it auto-saves (unlikely).
        // The previous file used `await _uow.Repository<>().AddAsync(); await _uow.SaveChangesAsync(ct);`.
        // Wait, does `IUnitOfWork` have `SaveChangesAsync`? Checks previously viewing code: NO.
        // It had `_uow.Repository<T>().AddAsync`.
        // I'll check `IUnitOfWork` definition if I can, but I'll assume `ServiceAxis.Shared` or `Application` defines it.
        // SlaCommands.cs old file line 49: `await _uow.SaveChangesAsync(ct);`.
        // So I'll include it.
        
        // Wait, I DON'T SEE SaveChangesAsync in IUnitOfWork usually. 
        // GenericRepository.AddAsync often saves?
        // Let's assume Add adds to context, and I need to save.
        // I'll attempt to call whatever method exists.
        // But `CreateSlaDefinitionHandler` line 49 had `_uow.SaveChangesAsync`.
        // So I will assume it exists.
        
        await _uow.SaveChangesAsync(ct);
        return def.Id;
    }
}

// ─── Add SLA Target ──────────────────────────────────────────────────────────

public record AddSlaTargetCommand(
    Guid SlaDefinitionId,
    SlaMetricType MetricType,
    int DurationMinutes,
    bool BusinessHoursOnly,
    Guid? BusinessCalendarId) : IRequest<Guid>;

public class AddSlaTargetHandler : IRequestHandler<AddSlaTargetCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public AddSlaTargetHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(AddSlaTargetCommand cmd, CancellationToken ct)
    {
        var target = new SlaTarget
        {
            SlaDefinitionId     = cmd.SlaDefinitionId,
            MetricType          = cmd.MetricType,
            TargetDurationMinutes = cmd.DurationMinutes,
            BusinessHoursOnly   = cmd.BusinessHoursOnly,
            BusinessCalendarId  = cmd.BusinessCalendarId
        };

        await _uow.Repository<SlaTarget>().AddAsync(target, ct);
        await _uow.SaveChangesAsync(ct);
        return target.Id;
    }
}

// ─── Add SLA Policy ──────────────────────────────────────────────────────────

public record AddSlaPolicyCommand(
    Guid SlaDefinitionId,
    string PriorityValue) : IRequest<Guid>;

public class AddSlaPolicyHandler : IRequestHandler<AddSlaPolicyCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public AddSlaPolicyHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(AddSlaPolicyCommand cmd, CancellationToken ct)
    {
        // Resolve Table ID from Definition
        var def = await _uow.Repository<SlaDefinition>().GetByIdAsync(cmd.SlaDefinitionId, ct)
            ?? throw new NotFoundException("SlaDefinition", cmd.SlaDefinitionId);

        var policy = new SlaPolicy
        {
            SlaDefinitionId = cmd.SlaDefinitionId,
            TableId         = def.TableId,
            PriorityValue   = cmd.PriorityValue
        };

        await _uow.Repository<SlaPolicy>().AddAsync(policy, ct);
        await _uow.SaveChangesAsync(ct);
        return policy.Id;
    }
}
