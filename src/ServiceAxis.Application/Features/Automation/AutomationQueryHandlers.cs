using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Enums;
using System.Text.Json;
using System.Linq;

namespace ServiceAxis.Application.Features.Automation;

public class AutomationQueryHandlers : 
    IRequestHandler<GetAutomationRulesQuery, PagedResult<AutomationRuleDto>>,
    IRequestHandler<GetAutomationRuleByIdQuery, AutomationRuleDto?>
{
    private readonly IUnitOfWork _uow;

    public AutomationQueryHandlers(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<AutomationRuleDto>> Handle(GetAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        var ruleRepo = _uow.Repository<AutomationRule>();

        var pagedRules = await ruleRepo.GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate: r => (!request.TableId.HasValue || r.TableId == request.TableId.Value) &&
                            (!request.IsActive.HasValue || r.IsActive == request.IsActive.Value),
            cancellationToken: cancellationToken);

        var dtos = new List<AutomationRuleDto>();
        var triggerRepo = _uow.Repository<AutomationTrigger>();
        var conditionRepo = _uow.Repository<AutomationCondition>();
        var actionRepo = _uow.Repository<AutomationAction>();

        foreach (var rule in pagedRules.Items)
        {
            var triggers = await triggerRepo.FindAsync(t => t.RuleId == rule.Id, cancellationToken);
            var conditions = await conditionRepo.FindAsync(c => c.RuleId == rule.Id, cancellationToken);
            var actions = await actionRepo.FindAsync(a => a.RuleId == rule.Id, cancellationToken);
            
            rule.Triggers = triggers.ToList();
            rule.Conditions = conditions.ToList();
            rule.Actions = actions.ToList();

            dtos.Add(MapToDto(rule));
        }

        return new PagedResult<AutomationRuleDto>
        {
            Items = dtos,
            TotalCount = pagedRules.TotalCount,
            PageNumber = pagedRules.PageNumber,
            PageSize = pagedRules.PageSize
        };
    }

    public async Task<AutomationRuleDto?> Handle(GetAutomationRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var ruleRepo = _uow.Repository<AutomationRule>();
        var rule = await ruleRepo.GetByIdAsync(request.Id, cancellationToken);

        if (rule != null)
        {
            var triggers = await _uow.Repository<AutomationTrigger>().FindAsync(t => t.RuleId == rule.Id, cancellationToken);
            var conditions = await _uow.Repository<AutomationCondition>().FindAsync(c => c.RuleId == rule.Id, cancellationToken);
            var actions = await _uow.Repository<AutomationAction>().FindAsync(a => a.RuleId == rule.Id, cancellationToken);
            
            rule.Triggers = triggers.ToList();
            rule.Conditions = conditions.ToList();
            rule.Actions = actions.ToList();
            return MapToDto(rule);
        }

        return null;
    }

    private static AutomationRuleDto MapToDto(AutomationRule rule)
    {
        return new AutomationRuleDto(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.TableId,
            rule.ExecutionMode,
            rule.StopProcessingOnMatch,
            rule.IsActive,
            rule.Triggers.Select(t => new AutomationTriggerDto(t.Id, t.EventType)).ToList(),
            rule.Conditions.Select(c => new AutomationConditionDto(c.Id, c.FieldId, c.Operator, c.Value, c.LogicalGroup)).ToList(),
            rule.Actions.Select(a => new AutomationActionDto(a.Id, a.ActionType, string.IsNullOrEmpty(a.ConfigurationJson) ? JsonDocument.Parse("{}") : JsonDocument.Parse(a.ConfigurationJson))).ToList()
        );
    }
}
