using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Metadata.Commands;

// ─── Create SysTable ─────────────────────────────────────────────────────────

public record CreateSysTableCommand(
    string Name,
    string DisplayName,
    string SchemaName,
    string? Description,
    string? Icon,
    string? AutoNumberPrefix,
    bool AuditEnabled,
    bool AllowAttachments,
    Guid? ParentTableId,
    Guid? TenantId) : IRequest<CreateSysTableResult>;

public record CreateSysTableResult(Guid Id, string Name, string DisplayName);

public class CreateSysTableHandler : IRequestHandler<CreateSysTableCommand, CreateSysTableResult>
{
    private readonly ISysTableRepository _tables;
    private readonly IUnitOfWork _uow;

    public CreateSysTableHandler(ISysTableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow    = uow;
    }

    public async Task<CreateSysTableResult> Handle(CreateSysTableCommand cmd, CancellationToken ct)
    {
        if (await _tables.ExistsAsync(cmd.Name, ct))
            throw new ConflictException($"A table named '{cmd.Name}' already exists.");

        var table = new SysTable
        {
            Name               = cmd.Name.ToLowerInvariant().Trim(),
            DisplayName        = cmd.DisplayName.Trim(),
            SchemaName         = cmd.SchemaName.ToLowerInvariant().Trim(),
            Description        = cmd.Description,
            Icon               = cmd.Icon,
            AutoNumberPrefix   = cmd.AutoNumberPrefix?.ToUpperInvariant(),
            AuditEnabled       = cmd.AuditEnabled,
            AllowAttachments   = cmd.AllowAttachments,
            ParentTableId      = cmd.ParentTableId,
            TenantId           = cmd.TenantId
        };

        await _tables.AddAsync(table, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreateSysTableResult(table.Id, table.Name, table.DisplayName);
    }
}

// ─── Add SysField ─────────────────────────────────────────────────────────────

public record AddSysFieldCommand(
    string TableName,
    string FieldName,
    string DisplayName,
    string DataType,
    bool IsRequired,
    string? DefaultValue,
    bool IsSearchable,
    string? ChoiceOptions,
    string? LookupTableName,
    int DisplayOrder,
    string? HelpText,
    Guid? TenantId) : IRequest<AddSysFieldResult>;

public record AddSysFieldResult(Guid Id, string FieldName, string DataType);

public class AddSysFieldHandler : IRequestHandler<AddSysFieldCommand, AddSysFieldResult>
{
    private readonly ISysTableRepository _tables;
    private readonly ISysFieldRepository _fields;
    private readonly IUnitOfWork _uow;

    public AddSysFieldHandler(ISysTableRepository tables, ISysFieldRepository fields, IUnitOfWork uow)
    {
        _tables = tables;
        _fields = fields;
        _uow    = uow;
    }

    public async Task<AddSysFieldResult> Handle(AddSysFieldCommand cmd, CancellationToken ct)
    {
        var table = await _tables.GetByNameAsync(cmd.TableName, ct)
            ?? throw new NotFoundException($"Table '{cmd.TableName}' not found.");

        var existing = await _fields.GetByTableAndNameAsync(table.Id, cmd.FieldName, ct);
        if (existing is not null)
            throw new ConflictException($"Field '{cmd.FieldName}' already exists on table '{cmd.TableName}'.");

        if (!Enum.TryParse<ServiceAxis.Domain.Enums.FieldDataType>(cmd.DataType, true, out var dataType))
            throw new BusinessException($"Unknown data type '{cmd.DataType}'.");

        var field = new SysField
        {
            TableId       = table.Id,
            FieldName     = cmd.FieldName.ToLowerInvariant().Trim(),
            DisplayName   = cmd.DisplayName.Trim(),
            DataType      = dataType,
            IsRequired    = cmd.IsRequired,
            DefaultValue  = cmd.DefaultValue,
            IsSearchable  = cmd.IsSearchable,
            ChoiceOptions = cmd.ChoiceOptions,
            LookupTableName = cmd.LookupTableName,
            DisplayOrder  = cmd.DisplayOrder,
            HelpText      = cmd.HelpText,
            TenantId      = cmd.TenantId
        };

        await _fields.AddAsync(field, ct);
        await _uow.SaveChangesAsync(ct);

        return new AddSysFieldResult(field.Id, field.FieldName, field.DataType.ToString());
    }
}
