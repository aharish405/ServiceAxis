using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Records.Commands;

// ─── Create Record ────────────────────────────────────────────────────────────

public record CreateRecordCommand(
    string TableName,
    Dictionary<string, string?> FieldValues,
    Guid? TenantId) : IRequest<RecordResult>;

public record RecordResult(
    Guid Id,
    string TableName,
    string? RecordNumber,
    string State,
    string? ShortDescription,
    Dictionary<string, string?> Values,
    DateTime CreatedAt);

public class CreateRecordHandler : IRequestHandler<CreateRecordCommand, RecordResult>
{
    private readonly ISysTableRepository   _tables;
    private readonly ISysFieldRepository   _fields;
    private readonly IRecordRepository     _records;
    private readonly IRecordValueRepository _values;
    private readonly ISlaService           _sla;
    private readonly IAssignmentService    _assignment;
    private readonly IUnitOfWork           _uow;

    public CreateRecordHandler(
        ISysTableRepository tables,
        ISysFieldRepository fields,
        IRecordRepository records,
        IRecordValueRepository values,
        ISlaService sla,
        IAssignmentService assignment,
        IUnitOfWork uow)
    {
        _tables  = tables;
        _fields  = fields;
        _records = records;
        _values  = values;
        _sla     = sla;
        _assignment = assignment;
        _uow     = uow;
    }

    public async Task<RecordResult> Handle(CreateRecordCommand cmd, CancellationToken ct)
    {
        var table = await _tables.GetWithFieldsAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        // Validate required fields
        var requiredFields = table.Fields.Where(f => f.IsRequired && f.IsActive).ToList();
        var missingFields  = requiredFields
            .Where(f => !cmd.FieldValues.ContainsKey(f.FieldName) || string.IsNullOrWhiteSpace(cmd.FieldValues[f.FieldName]))
            .Select(f => f.DisplayName).ToList();

        if (missingFields.Any())
            throw new BusinessException($"Missing required fields: {string.Join(", ", missingFields)}");

        // Resolve short description from the first text field or "title"/"name" field
        var titleField = table.Fields.FirstOrDefault(f =>
            f.FieldName is "title" or "name" or "short_description" or "subject");
        var shortDesc = titleField is not null && cmd.FieldValues.TryGetValue(titleField.FieldName, out var t)
            ? t : null;

        // Generate record number
        var recordNumber = table.AutoNumberPrefix is not null
            ? await _records.GenerateRecordNumberAsync(table.Name, ct)
            : null;

        // Parse priority
        int priority = 3; // Medium default
        if (cmd.FieldValues.TryGetValue("priority", out var priStr) && int.TryParse(priStr, out var pri))
            priority = pri;

        var record = new PlatformRecord
        {
            TableId          = table.Id,
            RecordNumber     = recordNumber,
            State            = "new",
            ShortDescription = shortDesc,
            Priority         = priority,
            TenantId         = cmd.TenantId
        };

        await _records.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        // Persist EAV values
        var fieldMap = table.Fields.ToDictionary(f => f.FieldName, f => f);
        foreach (var (key, val) in cmd.FieldValues)
        {
            if (!fieldMap.TryGetValue(key, out var fd)) continue;
            await _values.UpsertValueAsync(record.Id, fd.Id, val, ct);
        }
        await _uow.SaveChangesAsync(ct);

        // Start SLA if applicable
        await _sla.StartSlaAsync(record.Id, table.Name, priority, ct);

        // Auto-assign if not manually assigned
        if (string.IsNullOrEmpty(cmd.FieldValues.GetValueOrDefault("assigned_to")))
        {
            await _assignment.AutoAssignAsync(record.Id, table.Name, priority, ct);
        }

        return await BuildResultAsync(record, cmd.TableName, cmd.FieldValues);
    }

    private static Task<RecordResult> BuildResultAsync(PlatformRecord r, string tableName, Dictionary<string, string?> vals) =>
        Task.FromResult(new RecordResult(r.Id, tableName, r.RecordNumber, r.State, r.ShortDescription, vals, r.CreatedAt));
}

// ─── Update Record ────────────────────────────────────────────────────────────

public record UpdateRecordCommand(
    Guid RecordId,
    string TableName,
    Dictionary<string, string?> FieldValues) : IRequest<RecordResult>;

public class UpdateRecordHandler : IRequestHandler<UpdateRecordCommand, RecordResult>
{
    private readonly ISysTableRepository    _tables;
    private readonly IRecordRepository      _records;
    private readonly IRecordValueRepository _values;
    private readonly ISlaService            _sla;
    private readonly IUnitOfWork            _uow;

    public UpdateRecordHandler(
        ISysTableRepository tables,
        IRecordRepository records,
        IRecordValueRepository values,
        ISlaService sla,
        IUnitOfWork uow)
    {
        _tables  = tables;
        _records = records;
        _values  = values;
        _sla     = sla;
        _uow     = uow;
    }

    public async Task<RecordResult> Handle(UpdateRecordCommand cmd, CancellationToken ct)
    {
        var table = await _tables.GetWithFieldsAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var record = await _records.GetWithValuesAsync(cmd.RecordId, ct)
            ?? throw new NotFoundException($"Record '{cmd.RecordId}' not found.");

        var fieldMap = table.Fields.ToDictionary(f => f.FieldName, f => f);
        foreach (var (key, val) in cmd.FieldValues)
        {
            if (!fieldMap.TryGetValue(key, out var fd)) continue;
            await _values.UpsertValueAsync(record.Id, fd.Id, val, ct);
        }

        // Update state if provided
        if (cmd.FieldValues.TryGetValue("state", out var newState) && !string.IsNullOrWhiteSpace(newState) && newState != record.State)
        {
            // SLA Lifecycle Management based on State
            if (newState.Equals("resolved", StringComparison.OrdinalIgnoreCase) || newState.Equals("closed", StringComparison.OrdinalIgnoreCase))
            {
                await _sla.CompleteSlaAsync(record.Id, ct);
            }
            else if (newState.Equals("on_hold", StringComparison.OrdinalIgnoreCase) || newState.Contains("waiting", StringComparison.OrdinalIgnoreCase))
            {
                await _sla.PauseSlaAsync(record.Id, ct);
            }
            else if (record.State.Contains("waiting", StringComparison.OrdinalIgnoreCase) && !newState.Contains("waiting", StringComparison.OrdinalIgnoreCase))
            {
                // Resumed from waiting state
                await _sla.ResumeSlaAsync(record.Id, ct);
            }

            record.State = newState;
        }

        // Update short description
        var titleField = table.Fields.FirstOrDefault(f => f.FieldName is "title" or "name" or "short_description");
        if (titleField is not null && cmd.FieldValues.TryGetValue(titleField.FieldName, out var t))
            record.ShortDescription = t;

        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return new RecordResult(record.Id, cmd.TableName, record.RecordNumber, record.State,
            record.ShortDescription, cmd.FieldValues, record.CreatedAt);
    }
}
