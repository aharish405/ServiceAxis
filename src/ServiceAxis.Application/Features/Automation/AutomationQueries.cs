using MediatR;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Application.Features.Automation;

// ─── DTOs ──────────────────────────────────────────────────────────────────

public record AutomationRuleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid TableId,
    AutomationExecutionMode ExecutionMode,
    bool StopProcessingOnMatch,
    bool IsActive,
    List<AutomationTriggerDto> Triggers,
    List<AutomationConditionDto> Conditions,
    List<AutomationActionDto> Actions);

public record AutomationTriggerDto(
    Guid Id,
    AutomationEventType EventType);

public record AutomationConditionDto(
    Guid Id,
    Guid FieldId,
    AutomationOperator Operator,
    string? Value,
    LogicalGroup LogicalGroup);

public record AutomationActionDto(
    Guid Id,
    AutomationActionType ActionType,
    JsonDocument Configuration);


// ─── Queries ───────────────────────────────────────────────────────────────

public record GetAutomationRulesQuery(Guid? TableId, bool? IsActive, int Page = 1, int PageSize = 20) : IRequest<PagedResult<AutomationRuleDto>>;

public record GetAutomationRuleByIdQuery(Guid Id) : IRequest<AutomationRuleDto?>;
