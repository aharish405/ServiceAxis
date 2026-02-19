using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Records.Commands;

// ─── Delete (Soft) Record ─────────────────────────────────────────────────────

public record DeleteRecordCommand(string TableName, Guid RecordId) : IRequest;

public class DeleteRecordHandler : IRequestHandler<DeleteRecordCommand>
{
    private readonly ISysTableRepository _tables;
    private readonly IUnitOfWork _uow;

    public DeleteRecordHandler(ISysTableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow    = uow;
    }

    public async Task Handle(DeleteRecordCommand cmd, CancellationToken ct)
    {
        var table = await _tables.GetByNameAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var repo = _uow.Repository<PlatformRecord>();

        var record = await repo.GetByIdAsync(cmd.RecordId, ct)
            ?? throw new NotFoundException("Record", cmd.RecordId);

        if (record.TableId != table.Id)
            throw new BusinessException("Record does not belong to the specified table.");

        repo.SoftDelete(record);
        await _uow.SaveChangesAsync(ct);
    }
}

// ─── Assign Record ────────────────────────────────────────────────────────────

public record AssignRecordCommand(
    string TableName,
    Guid RecordId,
    string? AssignedToUserId,
    Guid? AssignedToGroupId) : IRequest;

public class AssignRecordHandler : IRequestHandler<AssignRecordCommand>
{
    private readonly ISysTableRepository _tables;
    private readonly IUnitOfWork _uow;
    private readonly IAssignmentService _assignment;

    public AssignRecordHandler(ISysTableRepository tables, IUnitOfWork uow, IAssignmentService assignment)
    {
        _tables     = tables;
        _uow        = uow;
        _assignment = assignment;
    }

    public async Task Handle(AssignRecordCommand cmd, CancellationToken ct)
    {
        var table = await _tables.GetByNameAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var record = await _uow.Repository<PlatformRecord>().GetByIdAsync(cmd.RecordId, ct)
            ?? throw new NotFoundException("Record", cmd.RecordId);

        if (record.TableId != table.Id)
            throw new BusinessException("Record does not belong to the specified table.");

        await _assignment.AssignAsync(
            record.Id,
            cmd.AssignedToUserId,
            cmd.AssignedToGroupId,
            ct);
    }
}
