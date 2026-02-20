using MediatR;
using ServiceAxis.Application.Common.Models;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Application.Features.Forms.Queries;

// ─── DTOs ──────────────────────────────────────────────────────────────────

public record UiMetadataPayloadDto(
    Guid TableId,
    FormLayoutDto? FormLayout,
    List<UiPolicyDto> UiPolicies,
    List<FieldRuleDto> FieldRules,
    List<ClientScriptDto> ClientScripts);

public record FormLayoutDto(
    Guid Id,
    string Name,
    string DisplayName,
    string FormContext,
    bool IsDefault,
    List<FormSectionDto> Sections);

public record FormSectionDto(
    Guid Id,
    string Title,
    int DisplayOrder,
    bool IsCollapsed,
    int Columns,
    List<FormFieldMappingDto> Fields);

public record FormFieldMappingDto(
    Guid Id,
    Guid FieldId,
    string FieldName,
    string FieldType,
    int DisplayOrder,
    bool? IsReadOnlyOverride,
    bool? IsRequiredOverride,
    bool IsHidden,
    string? LabelOverride,
    int ColSpan);

public record UiPolicyDto(
    Guid Id,
    string Name,
    int ExecutionOrder,
    FormContextType FormContext,
    int Version,
    List<UiPolicyConditionDto> Conditions,
    List<UiPolicyActionDto> Actions);

public record UiPolicyConditionDto(
    Guid FieldId,
    string FieldName,
    UiPolicyOperator Operator,
    string? Value,
    LogicalGroup LogicalGroup);

public record UiPolicyActionDto(
    Guid TargetFieldId,
    string TargetFieldName,
    UiPolicyActionType ActionType);

public record FieldRuleDto(
    Guid Id,
    string Name,
    Guid? TriggerFieldId,
    string? TriggerFieldName,
    JsonDocument ConditionJson,
    Guid TargetFieldId,
    string TargetFieldName,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    int Version);

public record ClientScriptDto(
    Guid Id,
    string Name,
    ClientScriptEventType EventType,
    Guid? TriggerFieldId,
    string? TriggerFieldName,
    string ScriptCode,
    int ExecutionOrder,
    int Version);

// ─── Queries ───────────────────────────────────────────────────────────────

/// <summary>
/// Retrieves the complete aggregated UI metadata payload (form layout + rules payload) 
/// acting as the single bootstrap payload for dynamic form rendering.
/// </summary>
public record GetUiMetadataQuery(Guid TableId, string FormContext = "default") : IRequest<UiMetadataPayloadDto>;
