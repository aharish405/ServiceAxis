using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Enums;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceAxis.Application.Features.Forms.Queries;

public sealed class GetUiMetadataQueryHandler : IRequestHandler<GetUiMetadataQuery, UiMetadataPayloadDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ServiceAxis.Application.Contracts.Infrastructure.IMetadataCache _metadata;
    private readonly IMemoryCache _memoryCache;

    public GetUiMetadataQueryHandler(IUnitOfWork uow, ServiceAxis.Application.Contracts.Infrastructure.IMetadataCache metadata, IMemoryCache memoryCache)
    {
        _uow = uow;
        _metadata = metadata;
        _memoryCache = memoryCache;
    }

    public async Task<UiMetadataPayloadDto> Handle(GetUiMetadataQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"ui-metadata-{request.TableId}-{request.FormContext.ToLower()}";

        if (_memoryCache.TryGetValue(cacheKey, out UiMetadataPayloadDto? cachedPayload) && cachedPayload != null)
        {
            return cachedPayload;
        }

        // 1. Fetch Form Layout
        var formDefRepo = _uow.Repository<FormDefinition>();
        var formDefs = await formDefRepo.FindAsync(fd => fd.TableId == request.TableId && fd.FormContext == request.FormContext, cancellationToken);
        var formDef = formDefs.FirstOrDefault(fd => fd.IsActive) ?? 
                      (await formDefRepo.FindAsync(fd => fd.TableId == request.TableId && fd.IsDefault, cancellationToken)).FirstOrDefault(fd => fd.IsActive);
        
        FormLayoutDto? layoutDto = null;
        if (formDef != null)
        {
            var sections = await _uow.Repository<FormSection>().FindAsync(s => s.FormDefinitionId == formDef.Id && s.IsActive, cancellationToken);
            var sectionDtos = new List<FormSectionDto>();
            
            var mappingsRepo = _uow.Repository<FormFieldMapping>();

            foreach(var section in sections.OrderBy(s => s.DisplayOrder))
            {
                var mappings = await mappingsRepo.FindAsync(m => m.FormSectionId == section.Id && m.IsActive, cancellationToken);
                var fieldDtos = new List<FormFieldMappingDto>();
                
                foreach(var mapping in mappings.OrderBy(m => m.DisplayOrder))
                {
                    var field = await _metadata.GetFieldAsync(mapping.FieldId, cancellationToken);
                    if (field != null)
                    {
                        fieldDtos.Add(new FormFieldMappingDto(
                            mapping.Id,
                            mapping.FieldId,
                            field.FieldName,
                            field.DataType.ToString(),
                            mapping.DisplayOrder,
                            mapping.IsReadOnlyOverride,
                            mapping.IsRequiredOverride,
                            mapping.IsHidden,
                            mapping.LabelOverride,
                            mapping.ColSpan
                        ));
                    }
                }

                sectionDtos.Add(new FormSectionDto(
                    section.Id,
                    section.Title,
                    section.DisplayOrder,
                    section.IsCollapsed,
                    section.Columns,
                    fieldDtos
                ));
            }

            layoutDto = new FormLayoutDto(
                formDef.Id,
                formDef.Name,
                formDef.DisplayName,
                formDef.FormContext,
                formDef.IsDefault,
                sectionDtos
            );
        }

        // 2. Fetch UI Policies
        // Note: For policies, map string-based 'request.FormContext' to the enum if required, 
        // Or simply pull 'FormContextType.All' + matching enum for create/edit.
        FormContextType enumContext = FormContextType.All;
        if (request.FormContext.Equals("create", StringComparison.OrdinalIgnoreCase)) enumContext = FormContextType.Create;
        if (request.FormContext.Equals("edit", StringComparison.OrdinalIgnoreCase)) enumContext = FormContextType.Edit;
        if (request.FormContext.Equals("view", StringComparison.OrdinalIgnoreCase)) enumContext = FormContextType.View;

        var allPolicies = await _uow.Repository<UiPolicy>()
            .FindAsync(p => p.TableId == request.TableId && p.IsActive && (p.FormContext == FormContextType.All || p.FormContext == enumContext), cancellationToken);
        var activePolicies = allPolicies.OrderBy(p => p.ExecutionOrder).ToList();
        
        var uowConditions = _uow.Repository<UiPolicyCondition>();
        var uowActions = _uow.Repository<UiPolicyAction>();

        var policiesResult = new List<UiPolicyDto>();

        foreach(var policy in activePolicies)
        {
            var conditions = await uowConditions.FindAsync(c => c.UiPolicyId == policy.Id && c.IsActive, cancellationToken);
            var actions = await uowActions.FindAsync(a => a.UiPolicyId == policy.Id && a.IsActive, cancellationToken);
            
            var conditionDtos = new List<UiPolicyConditionDto>();
            foreach (var c in conditions)
            {
                var field = await _metadata.GetFieldAsync(c.FieldId, cancellationToken);
                conditionDtos.Add(new UiPolicyConditionDto(
                    c.FieldId, field?.FieldName ?? "", c.Operator, c.Value, c.LogicalGroup));
            }

            var actionDtos = new List<UiPolicyActionDto>();
            foreach (var a in actions)
            {
                var targetField = await _metadata.GetFieldAsync(a.TargetFieldId, cancellationToken);
                actionDtos.Add(new UiPolicyActionDto(
                    a.TargetFieldId, targetField?.FieldName ?? "", a.ActionType));
            }

            policiesResult.Add(new UiPolicyDto(
                policy.Id,
                policy.Name,
                policy.ExecutionOrder,
                policy.FormContext,
                policy.Version,
                conditionDtos,
                actionDtos
            ));
        }

        // 3. Fetch Field Rules
        var allFieldRules = await _uow.Repository<FieldRule>()
            .FindAsync(f => f.TableId == request.TableId && f.IsActive, cancellationToken);
        var activeFieldRules = allFieldRules.OrderBy(f => f.ExecutionOrder).ToList();

        var fieldRulesResult = new List<FieldRuleDto>();
        foreach (var f in activeFieldRules)
        {
            var triggerField = f.TriggerFieldId.HasValue ? await _metadata.GetFieldAsync(f.TriggerFieldId.Value, cancellationToken) : null;
            var targetField = await _metadata.GetFieldAsync(f.TargetFieldId, cancellationToken);

            fieldRulesResult.Add(new FieldRuleDto(
                f.Id,
                f.Name,
                f.TriggerFieldId,
                triggerField?.FieldName ?? "",
                string.IsNullOrWhiteSpace(f.ConditionJson) ? JsonDocument.Parse("{}") : JsonDocument.Parse(f.ConditionJson),
                f.TargetFieldId,
                targetField?.FieldName ?? "",
                f.ActionType,
                f.ValueExpression,
                f.ExecutionOrder,
                f.Version
            ));
        }

        // 4. Fetch Client Scripts
        var allScripts = await _uow.Repository<ClientScript>()
            .FindAsync(s => s.TableId == request.TableId && s.IsActive, cancellationToken);
        var activeScripts = allScripts.OrderBy(s => s.ExecutionOrder).ToList();

        var scriptsResult = new List<ClientScriptDto>();
        foreach (var s in activeScripts)
        {
            var triggerField = s.TriggerFieldId.HasValue ? await _metadata.GetFieldAsync(s.TriggerFieldId.Value, cancellationToken) : null;
            scriptsResult.Add(new ClientScriptDto(
                s.Id,
                s.Name,
                s.EventType,
                s.TriggerFieldId,
                triggerField?.FieldName ?? "",
                s.ScriptCode,
                s.ExecutionOrder,
                s.Version
            ));
        }

        var payload = new UiMetadataPayloadDto(
            request.TableId,
            layoutDto,
            policiesResult,
            fieldRulesResult,
            scriptsResult
        );

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(1))
            .SetAbsoluteExpiration(TimeSpan.FromHours(12));

        _memoryCache.Set(cacheKey, payload, cacheOptions);

        return payload;
    }
}
