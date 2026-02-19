using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Shared.Exceptions;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Application.Contracts.Identity;

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
    private readonly IMetadataCache       _cache;
    private readonly IRecordRepository     _records;
    private readonly IRecordValueRepository _values;
    private readonly ISlaService           _sla;
    private readonly IAssignmentService    _assignment;
    private readonly IFieldTypeService     _fieldType;
    private readonly IActivityService      _activity;
    private readonly IPermissionService    _permission;
    private readonly IWorkflowEngine      _workflow;
    private readonly ICurrentUserService   _currentUser;
    private readonly IUnitOfWork           _uow;

    public CreateRecordHandler(
        IMetadataCache cache,
        IRecordRepository records,
        IRecordValueRepository values,
        ISlaService sla,
        IAssignmentService assignment,
        IFieldTypeService fieldType,
        IActivityService activity,
        IPermissionService permission,
        IWorkflowEngine workflow,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _cache   = cache;
        _records = records;
        _values  = values;
        _sla     = sla;
        _assignment = assignment;
        _fieldType = fieldType;
        _activity = activity;
        _permission = permission;
        _workflow = workflow;
        _currentUser = currentUser;
        _uow     = uow;
    }

    public async Task<RecordResult> Handle(CreateRecordCommand cmd, CancellationToken ct)
    {
        var table = await _cache.GetTableAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        // 0. Permission check
        if (!await _permission.CanAccessTableAsync(cmd.TableName, PermissionType.Create, ct))
            throw new ForbiddenException($"You do not have permission to create records in '{cmd.TableName}'.");

        // 1. Validate required fields and data types
        var requiredFields = table.Fields.Where(f => f.IsRequired && f.IsActive).ToList();
        var validationErrors = new List<string>();

        foreach (var field in table.Fields.Where(f => f.IsActive))
        {
            cmd.FieldValues.TryGetValue(field.FieldName, out var val);

            if (field.IsRequired && string.IsNullOrWhiteSpace(val))
            {
                validationErrors.Add($"Field '{field.DisplayName}' is required.");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(val) && !_fieldType.Validate(val, field.DataType, out var error))
            {
                validationErrors.Add($"Field '{field.DisplayName}': {error}");
            }
        }

        if (validationErrors.Any())
            throw new BusinessException($"Validation failed: {string.Join(" ", validationErrors)}");

        // 2. Resolve short description
        var titleField = table.Fields.FirstOrDefault(f =>
            f.FieldName is "title" or "name" or "short_description" or "subject" or "summary");
        var shortDesc = titleField is not null && cmd.FieldValues.TryGetValue(titleField.FieldName, out var t)
            ? t : null;

        // 3. Generate record number
        var recordNumber = table.AutoNumberPrefix is not null
            ? await _records.GenerateRecordNumberAsync(table.Name, ct)
            : null;

        // 4. Parse priority
        int priority = 3; 
        if (cmd.FieldValues.TryGetValue("priority", out var priStr) && int.TryParse(priStr, out var pri))
            priority = pri;

        var record = new PlatformRecord
        {
            TableId          = table.Id,
            RecordNumber     = recordNumber,
            State            = "new",
            ShortDescription = shortDesc,
            Priority         = priority,
            TenantId         = cmd.TenantId ?? _currentUser.TenantId
        };

        // 5. Unit of Work / Transaction
        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _records.AddAsync(record, ct);
            
            // Persist EAV values
            var fieldMap = table.Fields.ToDictionary(f => f.FieldName, f => f);
            foreach (var (key, val) in cmd.FieldValues)
            {
                if (!fieldMap.TryGetValue(key, out var fd)) continue;
                await _values.UpsertValueAsync(record.Id, fd.Id, val, ct);
            }

            // Platform Activity
            await _activity.LogActivityAsync(
                table.Id, 
                record.Id, 
                ActivityType.RecordCreated, 
                $"Record {record.RecordNumber} created.",
                isSystem: true,
                ct: ct);

            // Legacy Audit (optional, but kept for compatibility with existing queries)
            await _uow.Repository<RecordAudit>().AddAsync(new RecordAudit
            {
                RecordId = record.Id,
                Action = "Create",
                FieldName = "system",
                NewValue = "Record created",
                CreatedAt = DateTime.UtcNow
            }, ct);

            // Workflow Triggers
            await _workflow.ProcessTriggersAsync(table.Id, record.Id, WorkflowTriggerEvent.RecordCreated, null, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        // 6. Post-persistence logic
        await _sla.StartSlaAsync(record.Id, table.Name, priority, ct);

        if (string.IsNullOrEmpty(cmd.FieldValues.GetValueOrDefault("assigned_to")))
        {
            await _assignment.AutoAssignAsync(record.Id, table.Name, priority, ct);
        }

        return new RecordResult(record.Id, cmd.TableName, record.RecordNumber, record.State, record.ShortDescription, cmd.FieldValues, record.CreatedAt);
    }
}

// ─── Update Record ────────────────────────────────────────────────────────────

public record UpdateRecordCommand(
    Guid RecordId,
    string TableName,
    Dictionary<string, string?> FieldValues) : IRequest<RecordResult>;

public class UpdateRecordHandler : IRequestHandler<UpdateRecordCommand, RecordResult>
{
    private readonly IMetadataCache        _cache;
    private readonly IRecordRepository      _records;
    private readonly IRecordValueRepository _values;
    private readonly ISlaService           _sla;
    private readonly IFieldTypeService      _fieldType;
    private readonly IActivityService       _activity;
    private readonly IPermissionService     _permission;
    private readonly IWorkflowEngine       _workflow;
    private readonly IUnitOfWork           _uow;

    public UpdateRecordHandler(
        IMetadataCache cache,
        IRecordRepository records,
        IRecordValueRepository values,
        ISlaService sla,
        IFieldTypeService fieldType,
        IActivityService activity,
        IPermissionService permission,
        IWorkflowEngine workflow,
        IUnitOfWork uow)
    {
        _cache   = cache;
        _records = records;
        _values  = values;
        _sla     = sla;
        _fieldType = fieldType;
        _activity = activity;
        _permission = permission;
        _workflow = workflow;
        _uow     = uow;
    }

    public async Task<RecordResult> Handle(UpdateRecordCommand cmd, CancellationToken ct)
    {
        var table = await _cache.GetTableAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var record = await _records.GetWithValuesAsync(cmd.RecordId, ct)
            ?? throw new NotFoundException($"Record '{cmd.RecordId}' not found.");

        // 0. Permission check
        if (!await _permission.CanAccessTableAsync(cmd.TableName, PermissionType.Write, ct))
            throw new ForbiddenException($"You do not have permission to update records in '{cmd.TableName}'.");


        var oldValuesMap = record.Values.ToDictionary(v => v.Field.FieldName, v => v.Value);
        var fieldMap = table.Fields.ToDictionary(f => f.FieldName, f => f);
        var changes = new List<(string FieldName, Guid? FieldId, string? OldValue, string? NewValue)>();

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var (key, newVal) in cmd.FieldValues)
            {
                if (!fieldMap.TryGetValue(key, out var fd)) continue;
                
                // Field level write check
                if (!await _permission.CanAccessFieldAsync(cmd.TableName, key, isWriteRequest: true, ct))
                    continue; // Or throw? Requirements usually prefer silent skip or diagnostic error. Let's skip for flexibility.

                
                oldValuesMap.TryGetValue(key, out var oldVal);
                if (oldVal == newVal) continue;

                // Validate
                if (!_fieldType.Validate(newVal, fd.DataType, out var error))
                    throw new BusinessException($"Field '{fd.DisplayName}': {error}");

                await _values.UpsertValueAsync(record.Id, fd.Id, newVal, ct);

                changes.Add((fd.FieldName, fd.Id, oldVal, newVal));

                // Field Audit
                await _uow.Repository<RecordAudit>().AddAsync(new RecordAudit
                {
                    RecordId = record.Id,
                    FieldId = fd.Id,
                    FieldName = fd.FieldName,
                    OldValue = oldVal,
                    NewValue = newVal,
                    Action = "Update",
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            bool statusChanged = false;
            // Update cross-EAV fields (state, priority, etc.)
            if (cmd.FieldValues.TryGetValue("state", out var newState) && !string.IsNullOrWhiteSpace(newState) && newState != record.State)
            {
                if (newState.Equals("resolved", StringComparison.OrdinalIgnoreCase) || newState.Equals("closed", StringComparison.OrdinalIgnoreCase))
                    await _sla.CompleteSlaAsync(record.Id, ct);
                
                changes.Add(("state", null, record.State, newState));
                statusChanged = true;

                await _uow.Repository<RecordAudit>().AddAsync(new RecordAudit {
                    RecordId = record.Id, FieldName = "state", OldValue = record.State, NewValue = newState, Action = "Transition", CreatedAt = DateTime.UtcNow
                }, ct);

                record.State = newState;
            }

            if (cmd.FieldValues.TryGetValue("priority", out var priStr) && int.TryParse(priStr, out var pri) && pri != record.Priority)
            {
                changes.Add(("priority", null, record.Priority.ToString(), pri.ToString()));
                
                await _uow.Repository<RecordAudit>().AddAsync(new RecordAudit {
                    RecordId = record.Id, FieldName = "priority", OldValue = record.Priority.ToString(), NewValue = pri.ToString(), Action = "Update", CreatedAt = DateTime.UtcNow
                }, ct);
                record.Priority = pri;
            }

            // Log activity
            if (changes.Any())
            {
                await _activity.LogActivityAsync(
                    table.Id,
                    record.Id,
                    statusChanged ? ActivityType.StatusChanged : ActivityType.FieldChanged,
                    statusChanged ? $"Status changed to {record.State}" : "Fields updated",
                    isSystem: true,
                    changes: changes,
                    ct: ct);
            }

            var titleField = table.Fields.FirstOrDefault(f => f.FieldName is "title" or "name" or "short_description" or "subject" or "summary");
            if (titleField is not null && cmd.FieldValues.TryGetValue(titleField.FieldName, out var t))
                record.ShortDescription = t;

            record.UpdatedAt = DateTime.UtcNow;

            // Trigger generic RecordUpdated
            await _workflow.ProcessTriggersAsync(table.Id, record.Id, WorkflowTriggerEvent.RecordUpdated, null, ct);

            // Trigger specific FieldChanged / StateChanged
            foreach (var change in changes)
            {
                if (change.FieldName == "state")
                    await _workflow.ProcessTriggersAsync(table.Id, record.Id, WorkflowTriggerEvent.StateChanged, "state", ct);
                
                await _workflow.ProcessTriggersAsync(table.Id, record.Id, WorkflowTriggerEvent.FieldChanged, change.FieldName, ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        var finalValues = (await _records.GetWithValuesAsync(record.Id, ct))!.Values.ToDictionary(v => v.Field.FieldName, v => v.Value);

        return new RecordResult(record.Id, cmd.TableName, record.RecordNumber, record.State,
            record.ShortDescription, finalValues, record.CreatedAt);
    }
}
