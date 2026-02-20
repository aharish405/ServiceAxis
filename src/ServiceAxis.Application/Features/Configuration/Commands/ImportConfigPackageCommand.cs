using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Application.Features.Configuration.Models;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Platform;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Linq;

namespace ServiceAxis.Application.Features.Configuration.Commands;

public record ImportConfigPackageCommand(ConfigPackageDto Package, bool DryRun = true) : IRequest<ImportResultDto>;

public sealed class ImportConfigPackageCommandHandler : IRequestHandler<ImportConfigPackageCommand, ImportResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public ImportConfigPackageCommandHandler(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    public async Task<ImportResultDto> Handle(ImportConfigPackageCommand request, CancellationToken cancellationToken)
    {
        var messages = new List<string>();
        var errors = new List<string>();
        bool success = true;

        if (request.Package.Tables == null || !request.Package.Tables.Any())
        {
            errors.Add("Package contains no configuration tables.");
            return new ImportResultDto(false, request.DryRun, messages, errors);
        }

        foreach (var tableConfig in request.Package.Tables)
        {
            var tables = await _uow.Repository<SysTable>().FindAsync(t => t.Name == tableConfig.TableName, cancellationToken);
            var table = tables.FirstOrDefault();

            if (table == null)
            {
                errors.Add($"Target table '{tableConfig.TableName}' does not exist on this environment.");
                success = false;
                continue;
            }

            var fields = await _uow.Repository<SysField>().FindAsync(f => f.TableId == table.Id, cancellationToken);
            var fieldDict = fields.ToDictionary(f => f.FieldName.ToLower(), f => f.Id);

            messages.Add($"Table '{table.Name}' matched.");

            // Dependencies Validation Pass
            int missingDependenciesCount = 0;
            
            // Validate UI Policies dependencies
            if (tableConfig.UiPolicies != null)
            {
                foreach (var p in tableConfig.UiPolicies)
                {
                    foreach (var c in p.Conditions)
                    {
                        if (!fieldDict.ContainsKey(c.FieldName.ToLower())) { errors.Add($"UI Policy '{p.Name}' condition references missing field: {c.FieldName}"); missingDependenciesCount++; }
                    }
                    foreach (var a in p.Actions)
                    {
                        if (!fieldDict.ContainsKey(a.TargetFieldName.ToLower())) { errors.Add($"UI Policy '{p.Name}' action references missing field: {a.TargetFieldName}"); missingDependenciesCount++; }
                    }
                }
            }

            // Validate Field Rules dependencies
            if (tableConfig.FieldRules != null)
            {
                foreach (var r in tableConfig.FieldRules)
                {
                    if (r.TriggerFieldName != null && !fieldDict.ContainsKey(r.TriggerFieldName.ToLower())) { errors.Add($"Field Rule '{r.Name}' trigger references missing field: {r.TriggerFieldName}"); missingDependenciesCount++; }
                    if (!fieldDict.ContainsKey(r.TargetFieldName.ToLower())) { errors.Add($"Field Rule '{r.Name}' target references missing field: {r.TargetFieldName}"); missingDependenciesCount++; }
                }
            }

            // Validate Client Scripts dependencies
            if (tableConfig.ClientScripts != null)
            {
                foreach (var s in tableConfig.ClientScripts)
                {
                    if (s.TriggerFieldName != null && !fieldDict.ContainsKey(s.TriggerFieldName.ToLower())) { errors.Add($"Client Script '{s.Name}' trigger references missing field: {s.TriggerFieldName}"); missingDependenciesCount++; }
                }
            }

            if (missingDependenciesCount > 0)
            {
                success = false;
                messages.Add($"Aborting import for '{table.Name}' due to {missingDependenciesCount} broken dependencies.");
                continue;
            }

            if (!request.DryRun && success)
            {
                // Proceed with Import execution: (destructive update based on matching Names)
                if (tableConfig.UiPolicies != null) await ImportUiPolicies(table.Id, tableConfig.UiPolicies, fieldDict, cancellationToken);
                if (tableConfig.FieldRules != null) await ImportFieldRules(table.Id, tableConfig.FieldRules, fieldDict, cancellationToken);
                if (tableConfig.ClientScripts != null) await ImportClientScripts(table.Id, tableConfig.ClientScripts, fieldDict, cancellationToken);
                
                await _uow.SaveChangesAsync(cancellationToken);
                
                // Cache invalidate mappings
                var contexts = new[] { "default", "create", "edit", "view", "all" };
                foreach (var ctx in contexts)
                {
                    _memoryCache.Remove($"ui-metadata-{table.Id}-{ctx}");
                }

                messages.Add($"Successfully imported configuration logic for '{table.Name}'. App cache invalidated.");
            }
        }

        return new ImportResultDto(success, request.DryRun, messages, errors);
    }

    private async Task ImportUiPolicies(Guid tableId, List<UiPolicyExportDto> exports, Dictionary<string, Guid> fieldDict, CancellationToken ct)
    {
        var repo = _uow.Repository<UiPolicy>();
        var existingPolicies = await repo.FindAsync(p => p.TableId == tableId, ct);
        
        foreach (var export in exports)
        {
            var existing = existingPolicies.FirstOrDefault(p => p.Name == export.Name);
            if (existing != null)
            {
                existing.Description = export.Description;
                existing.ExecutionOrder = export.ExecutionOrder;
                existing.IsActive = export.IsActive;
                existing.FormContext = export.FormContext;
                existing.Version = export.Version + 1; // Bump implicitly during import

                var condRepo = _uow.Repository<UiPolicyCondition>();
                var conditions = await condRepo.FindAsync(c => c.UiPolicyId == existing.Id, ct);
                foreach (var c in conditions) condRepo.Delete(c);

                existing.Conditions = export.Conditions.Select(c => new UiPolicyCondition
                {
                    FieldId = fieldDict[c.FieldName.ToLower()],
                    Operator = c.Operator,
                    Value = c.Value,
                    LogicalGroup = c.LogicalGroup
                }).ToList();

                var actionRepo = _uow.Repository<UiPolicyAction>();
                var actions = await actionRepo.FindAsync(a => a.UiPolicyId == existing.Id, ct);
                foreach (var a in actions) actionRepo.Delete(a);

                existing.Actions = export.Actions.Select(a => new UiPolicyAction
                {
                    TargetFieldId = fieldDict[a.TargetFieldName.ToLower()],
                    ActionType = a.ActionType
                }).ToList();

                repo.Update(existing);
            }
            else
            {
                var newPolicy = new UiPolicy
                {
                    TableId = tableId,
                    Name = export.Name,
                    Description = export.Description,
                    ExecutionOrder = export.ExecutionOrder,
                    IsActive = export.IsActive,
                    FormContext = export.FormContext,
                    Version = export.Version,
                    Conditions = export.Conditions.Select(c => new UiPolicyCondition
                    {
                        FieldId = fieldDict[c.FieldName.ToLower()],
                        Operator = c.Operator,
                        Value = c.Value,
                        LogicalGroup = c.LogicalGroup
                    }).ToList(),
                    Actions = export.Actions.Select(a => new UiPolicyAction
                    {
                        TargetFieldId = fieldDict[a.TargetFieldName.ToLower()],
                        ActionType = a.ActionType
                    }).ToList()
                };
                await repo.AddAsync(newPolicy, ct);
            }
        }
    }

    private async Task ImportFieldRules(Guid tableId, List<FieldRuleExportDto> exports, Dictionary<string, Guid> fieldDict, CancellationToken ct)
    {
        var repo = _uow.Repository<FieldRule>();
        var existingRules = await repo.FindAsync(r => r.TableId == tableId, ct);

        foreach (var export in exports)
        {
            var existing = existingRules.FirstOrDefault(r => r.Name == export.Name);
            Guid? triggerId = export.TriggerFieldName != null ? fieldDict[export.TriggerFieldName.ToLower()] : null;
            Guid targetId = fieldDict[export.TargetFieldName.ToLower()];

            if (existing != null)
            {
                existing.TriggerFieldId = triggerId;
                existing.ConditionJson = export.ConditionJson.RootElement.ToString();
                existing.TargetFieldId = targetId;
                existing.ActionType = export.ActionType;
                existing.ValueExpression = export.ValueExpression;
                existing.ExecutionOrder = export.ExecutionOrder;
                existing.IsActive = export.IsActive;
                existing.Version = export.Version + 1;
                repo.Update(existing);
            }
            else
            {
                await repo.AddAsync(new FieldRule
                {
                    TableId = tableId,
                    Name = export.Name,
                    TriggerFieldId = triggerId,
                    ConditionJson = export.ConditionJson.RootElement.ToString(),
                    TargetFieldId = targetId,
                    ActionType = export.ActionType,
                    ValueExpression = export.ValueExpression,
                    ExecutionOrder = export.ExecutionOrder,
                    IsActive = export.IsActive,
                    Version = export.Version
                }, ct);
            }
        }
    }

    private async Task ImportClientScripts(Guid tableId, List<ClientScriptExportDto> exports, Dictionary<string, Guid> fieldDict, CancellationToken ct)
    {
        var repo = _uow.Repository<ClientScript>();
        var existingScripts = await repo.FindAsync(s => s.TableId == tableId, ct);

        foreach (var export in exports)
        {
            var existing = existingScripts.FirstOrDefault(s => s.Name == export.Name);
            Guid? triggerId = export.TriggerFieldName != null ? fieldDict[export.TriggerFieldName.ToLower()] : null;

            if (existing != null)
            {
                existing.Description = export.Description;
                existing.EventType = export.EventType;
                existing.TriggerFieldId = triggerId;
                existing.ScriptCode = export.ScriptCode;
                existing.ExecutionOrder = export.ExecutionOrder;
                existing.IsActive = export.IsActive;
                existing.Version = export.Version + 1;
                repo.Update(existing);
            }
            else
            {
                await repo.AddAsync(new ClientScript
                {
                    TableId = tableId,
                    Name = export.Name,
                    Description = export.Description,
                    EventType = export.EventType,
                    TriggerFieldId = triggerId,
                    ScriptCode = export.ScriptCode,
                    ExecutionOrder = export.ExecutionOrder,
                    IsActive = export.IsActive,
                    Version = export.Version
                }, ct);
            }
        }
    }
}
