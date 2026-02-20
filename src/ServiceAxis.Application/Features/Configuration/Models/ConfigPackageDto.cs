using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Application.Features.Configuration.Models;

public record ConfigPackageDto(
    string PackageVersion,
    DateTime ExportedAt,
    string SourceEnvironment,
    List<TableConfigExportDto> Tables);

public record TableConfigExportDto(
    string TableName,
    List<FormLayoutExportDto> FormLayouts,
    List<UiPolicyExportDto> UiPolicies,
    List<FieldRuleExportDto> FieldRules,
    List<ClientScriptExportDto> ClientScripts);

public record FormLayoutExportDto(
    string Name,
    string DisplayName,
    string FormContext,
    bool IsDefault,
    bool IsActive,
    List<FormSectionExportDto> Sections);

public record FormSectionExportDto(
    string Title,
    int DisplayOrder,
    bool IsCollapsed,
    int Columns,
    string? VisibilityCondition,
    List<FormFieldMappingExportDto> Fields);

public record FormFieldMappingExportDto(
    string FieldName,
    int DisplayOrder,
    bool? IsReadOnlyOverride,
    bool? IsRequiredOverride,
    bool IsHidden,
    string? LabelOverride,
    int ColSpan,
    string? VisibilityCondition);

public record UiPolicyExportDto(
    string Name,
    string? Description,
    int ExecutionOrder,
    bool IsActive,
    FormContextType FormContext,
    int Version,
    List<UiPolicyConditionExportDto> Conditions,
    List<UiPolicyActionExportDto> Actions);

public record UiPolicyConditionExportDto(
    string FieldName,
    UiPolicyOperator Operator,
    string? Value,
    LogicalGroup LogicalGroup);

public record UiPolicyActionExportDto(
    string TargetFieldName,
    UiPolicyActionType ActionType);

public record FieldRuleExportDto(
    string Name,
    string? TriggerFieldName,
    JsonDocument ConditionJson,
    string TargetFieldName,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    bool IsActive,
    int Version);

public record ClientScriptExportDto(
    string Name,
    string? Description,
    ClientScriptEventType EventType,
    string? TriggerFieldName,
    string ScriptCode,
    int ExecutionOrder,
    bool IsActive,
    int Version);

public record ImportResultDto(
    bool Success,
    bool IsDryRun,
    List<string> Messages,
    List<string> Errors);
