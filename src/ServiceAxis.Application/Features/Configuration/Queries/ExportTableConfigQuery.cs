using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Application.Features.Configuration.Models;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Platform;
using System.Text.Json;
using System.Linq;

namespace ServiceAxis.Application.Features.Configuration.Queries;

public record ExportTableConfigQuery(string TableName) : IRequest<ConfigPackageDto>;

public sealed class ExportTableConfigQueryHandler : IRequestHandler<ExportTableConfigQuery, ConfigPackageDto>
{
    private readonly IUnitOfWork _uow;

    public ExportTableConfigQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ConfigPackageDto> Handle(ExportTableConfigQuery request, CancellationToken cancellationToken)
    {
        var tables = await _uow.Repository<SysTable>().FindAsync(t => t.Name == request.TableName, cancellationToken);
        var table = tables.FirstOrDefault();
        
        if (table == null)
            throw new Exception($"Table {request.TableName} not found.");

        var fields = await _uow.Repository<SysField>().FindAsync(f => f.TableId == table.Id, cancellationToken);
        var fieldDict = fields.ToDictionary(f => f.Id, f => f.FieldName);

        var tableExport = new TableConfigExportDto(
            table.Name,
            await ExportFormLayouts(table.Id, fieldDict, cancellationToken),
            await ExportUiPolicies(table.Id, fieldDict, cancellationToken),
            await ExportFieldRules(table.Id, fieldDict, cancellationToken),
            await ExportClientScripts(table.Id, fieldDict, cancellationToken)
        );

        return new ConfigPackageDto(
            "1.0",
            DateTime.UtcNow,
            "EnvironmentName", // Ideally pulled from options
            new List<TableConfigExportDto> { tableExport }
        );
    }

    private async Task<List<FormLayoutExportDto>> ExportFormLayouts(Guid tableId, Dictionary<Guid, string> fieldDict, CancellationToken ct)
    {
        var formDefRepo = _uow.Repository<FormDefinition>();
        var formDefs = await formDefRepo.FindAsync(fd => fd.TableId == tableId, ct);
        var result = new List<FormLayoutExportDto>();

        foreach (var formDef in formDefs)
        {
            var sections = await _uow.Repository<FormSection>().FindAsync(s => s.FormDefinitionId == formDef.Id, ct);
            var sectionExports = new List<FormSectionExportDto>();

            var mappingRepo = _uow.Repository<FormFieldMapping>();

            foreach (var section in sections.OrderBy(s => s.DisplayOrder))
            {
                var mappings = await mappingRepo.FindAsync(m => m.FormSectionId == section.Id, ct);
                var fieldExports = new List<FormFieldMappingExportDto>();

                foreach (var mapping in mappings.OrderBy(m => m.DisplayOrder))
                {
                    if (fieldDict.TryGetValue(mapping.FieldId, out var fieldName))
                    {
                        fieldExports.Add(new FormFieldMappingExportDto(
                            fieldName,
                            mapping.DisplayOrder,
                            mapping.IsReadOnlyOverride,
                            mapping.IsRequiredOverride,
                            mapping.IsHidden,
                            mapping.LabelOverride,
                            mapping.ColSpan,
                            mapping.VisibilityCondition
                        ));
                    }
                }

                sectionExports.Add(new FormSectionExportDto(
                    section.Title,
                    section.DisplayOrder,
                    section.IsCollapsed,
                    section.Columns,
                    section.VisibilityCondition,
                    fieldExports
                ));
            }

            result.Add(new FormLayoutExportDto(
                formDef.Name,
                formDef.DisplayName,
                formDef.FormContext,
                formDef.IsDefault,
                formDef.IsActive,
                sectionExports
            ));
        }

        return result;
    }

    private async Task<List<UiPolicyExportDto>> ExportUiPolicies(Guid tableId, Dictionary<Guid, string> fieldDict, CancellationToken ct)
    {
        var policies = await _uow.Repository<UiPolicy>().FindAsync(p => p.TableId == tableId, ct);
        var result = new List<UiPolicyExportDto>();

        var condRepo = _uow.Repository<UiPolicyCondition>();
        var actionRepo = _uow.Repository<UiPolicyAction>();

        foreach (var policy in policies)
        {
            var conditions = await condRepo.FindAsync(c => c.UiPolicyId == policy.Id, ct);
            var actions = await actionRepo.FindAsync(a => a.UiPolicyId == policy.Id, ct);

            var condExports = conditions
                .Where(c => fieldDict.ContainsKey(c.FieldId))
                .Select(c => new UiPolicyConditionExportDto(fieldDict[c.FieldId], c.Operator, c.Value, c.LogicalGroup))
                .ToList();

            var actionExports = actions
                .Where(a => fieldDict.ContainsKey(a.TargetFieldId))
                .Select(a => new UiPolicyActionExportDto(fieldDict[a.TargetFieldId], a.ActionType))
                .ToList();

            result.Add(new UiPolicyExportDto(
                policy.Name,
                policy.Description,
                policy.ExecutionOrder,
                policy.IsActive,
                policy.FormContext,
                policy.Version,
                condExports,
                actionExports
            ));
        }

        return result;
    }

    private async Task<List<FieldRuleExportDto>> ExportFieldRules(Guid tableId, Dictionary<Guid, string> fieldDict, CancellationToken ct)
    {
        var rules = await _uow.Repository<FieldRule>().FindAsync(p => p.TableId == tableId, ct);
        var result = new List<FieldRuleExportDto>();

        foreach (var r in rules)
        {
            if (fieldDict.TryGetValue(r.TargetFieldId, out var targetName))
            {
                string? triggerName = r.TriggerFieldId.HasValue && fieldDict.TryGetValue(r.TriggerFieldId.Value, out var tName) ? tName : null;
                
                result.Add(new FieldRuleExportDto(
                    r.Name,
                    triggerName,
                    string.IsNullOrWhiteSpace(r.ConditionJson) ? JsonDocument.Parse("{}") : JsonDocument.Parse(r.ConditionJson),
                    targetName,
                    r.ActionType,
                    r.ValueExpression,
                    r.ExecutionOrder,
                    r.IsActive,
                    r.Version
                ));
            }
        }
        return result;
    }

    private async Task<List<ClientScriptExportDto>> ExportClientScripts(Guid tableId, Dictionary<Guid, string> fieldDict, CancellationToken ct)
    {
        var scripts = await _uow.Repository<ClientScript>().FindAsync(p => p.TableId == tableId, ct);
        var result = new List<ClientScriptExportDto>();

        foreach (var s in scripts)
        {
            string? triggerName = s.TriggerFieldId.HasValue && fieldDict.TryGetValue(s.TriggerFieldId.Value, out var tName) ? tName : null;

            result.Add(new ClientScriptExportDto(
                s.Name,
                s.Description,
                s.EventType,
                triggerName,
                s.ScriptCode,
                s.ExecutionOrder,
                s.IsActive,
                s.Version
            ));
        }
        return result;
    }
}
