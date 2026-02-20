using MediatR;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Application.Features.Forms.Commands;

// ─── UI Policy Commands ──────────────────────────────────────────────────

public record CreateUiPolicyCommand(
    Guid TableId,
    string Name,
    string? Description,
    int ExecutionOrder,
    bool IsActive,
    FormContextType FormContext,
    List<CreateUiPolicyConditionDto> Conditions,
    List<CreateUiPolicyActionDto> Actions) : IRequest<Guid>;

public record UpdateUiPolicyCommand(
    Guid Id,
    string Name,
    string? Description,
    int ExecutionOrder,
    bool IsActive,
    FormContextType FormContext,
    int Version,
    List<CreateUiPolicyConditionDto> Conditions,
    List<CreateUiPolicyActionDto> Actions) : IRequest<Unit>;

public record DeleteUiPolicyCommand(Guid Id) : IRequest<Unit>;

public record CreateUiPolicyConditionDto(
    Guid FieldId,
    UiPolicyOperator Operator,
    string? Value,
    LogicalGroup LogicalGroup);

public record CreateUiPolicyActionDto(
    Guid TargetFieldId,
    UiPolicyActionType ActionType);

// ─── Field Rule Commands ─────────────────────────────────────────────────

public record CreateFieldRuleCommand(
    Guid TableId,
    string Name,
    Guid? TriggerFieldId,
    JsonDocument Condition,
    Guid TargetFieldId,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    bool IsActive) : IRequest<Guid>;

public record UpdateFieldRuleCommand(
    Guid Id,
    string Name,
    Guid? TriggerFieldId,
    JsonDocument Condition,
    Guid TargetFieldId,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    bool IsActive,
    int Version) : IRequest<Unit>;

public record DeleteFieldRuleCommand(Guid Id) : IRequest<Unit>;

// ─── Client Script Commands ──────────────────────────────────────────────

public record CreateClientScriptCommand(
    Guid TableId,
    string Name,
    string? Description,
    ClientScriptEventType EventType,
    Guid? TriggerFieldId,
    string ScriptCode,
    int ExecutionOrder,
    bool IsActive) : IRequest<Guid>;

public record UpdateClientScriptCommand(
    Guid Id,
    string Name,
    string? Description,
    ClientScriptEventType EventType,
    Guid? TriggerFieldId,
    string ScriptCode,
    int ExecutionOrder,
    bool IsActive,
    int Version) : IRequest<Unit>;

public record DeleteClientScriptCommand(Guid Id) : IRequest<Unit>;
