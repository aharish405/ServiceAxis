using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Shared.Exceptions;
using System.Linq;

namespace ServiceAxis.Application.Features.Automation;

public class AutomationCommandHandlers :
    IRequestHandler<CreateAutomationRuleCommand, Guid>,
    IRequestHandler<UpdateAutomationRuleCommand, Unit>,
    IRequestHandler<ActivateAutomationRuleCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public AutomationCommandHandlers(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = new AutomationRule
        {
            Name = request.Name,
            Description = request.Description,
            TableId = request.TableId,
            ExecutionMode = request.ExecutionMode,
            StopProcessingOnMatch = request.StopProcessingOnMatch,
            IsActive = request.IsActive,
            Triggers = request.Triggers.Select(t => new AutomationTrigger { EventType = t.EventType }).ToList(),
            Conditions = request.Conditions.Select(c => new AutomationCondition
            {
                FieldId = c.FieldId,
                Operator = c.Operator,
                Value = c.Value,
                LogicalGroup = c.LogicalGroup
            }).ToList(),
            Actions = request.Actions.Select(a => new AutomationAction
            {
                ActionType = a.ActionType,
                ConfigurationJson = a.Configuration.RootElement.ToString()
            }).ToList()
        };

        var repo = _uow.Repository<AutomationRule>();
        await repo.AddAsync(rule, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }

    public async Task<Unit> Handle(UpdateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<AutomationRule>();

        var rule = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (rule == null)
            throw new NotFoundException(nameof(AutomationRule), request.Id);

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.ExecutionMode = request.ExecutionMode;
        rule.StopProcessingOnMatch = request.StopProcessingOnMatch;
        rule.IsActive = request.IsActive;

        var triggerRepo = _uow.Repository<AutomationTrigger>();
        var triggers = await triggerRepo.FindAsync(t => t.RuleId == request.Id, cancellationToken);
        foreach (var t in triggers) triggerRepo.Delete(t);

        rule.Triggers = request.Triggers.Select(t => new AutomationTrigger { EventType = t.EventType }).ToList();

        var conditionRepo = _uow.Repository<AutomationCondition>();
        var conditions = await conditionRepo.FindAsync(c => c.RuleId == request.Id, cancellationToken);
        foreach (var c in conditions) conditionRepo.Delete(c);

        rule.Conditions = request.Conditions.Select(c => new AutomationCondition
        {
            FieldId = c.FieldId,
            Operator = c.Operator,
            Value = c.Value,
            LogicalGroup = c.LogicalGroup
        }).ToList();

        var actionRepo = _uow.Repository<AutomationAction>();
        var actions = await actionRepo.FindAsync(a => a.RuleId == request.Id, cancellationToken);
        foreach (var a in actions) actionRepo.Delete(a);

        rule.Actions = request.Actions.Select(a => new AutomationAction
        {
            ActionType = a.ActionType,
            ConfigurationJson = a.Configuration.RootElement.ToString()
        }).ToList();

        repo.Update(rule);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    public async Task<Unit> Handle(ActivateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<AutomationRule>();
        var rule = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (rule == null)
            throw new NotFoundException(nameof(AutomationRule), request.Id);

        rule.IsActive = request.IsActive;
        repo.Update(rule);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
