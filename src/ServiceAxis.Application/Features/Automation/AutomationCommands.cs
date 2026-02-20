using MediatR;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Application.Features.Automation;

// ─── Commands ──────────────────────────────────────────────────────────────

public record CreateAutomationRuleCommand(
    string Name,
    string? Description,
    Guid TableId,
    AutomationExecutionMode ExecutionMode,
    bool StopProcessingOnMatch,
    bool IsActive,
    List<CreateAutomationTriggerDto> Triggers,
    List<CreateAutomationConditionDto> Conditions,
    List<CreateAutomationActionDto> Actions) : IRequest<Guid>;

public record UpdateAutomationRuleCommand(
    Guid Id,
    string Name,
    string? Description,
    AutomationExecutionMode ExecutionMode,
    bool StopProcessingOnMatch,
    bool IsActive,
    List<CreateAutomationTriggerDto> Triggers,
    List<CreateAutomationConditionDto> Conditions,
    List<CreateAutomationActionDto> Actions) : IRequest<Unit>;

public record ActivateAutomationRuleCommand(Guid Id, bool IsActive) : IRequest<Unit>;


// ─── Support DTOs ──────────────────────────────────────────────────────────

public record CreateAutomationTriggerDto(
    AutomationEventType EventType);

public record CreateAutomationConditionDto(
    Guid FieldId,
    AutomationOperator Operator,
    string? Value,
    LogicalGroup LogicalGroup);

public record CreateAutomationActionDto(
    AutomationActionType ActionType,
    JsonDocument Configuration);
