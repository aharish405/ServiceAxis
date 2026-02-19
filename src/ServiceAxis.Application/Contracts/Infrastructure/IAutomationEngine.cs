using ServiceAxis.Application.Common.Models;
using ServiceAxis.Domain.Entities.Automation;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IAutomationEngine
{
    Task ProcessEventAsync<T>(T @event) where T : PlatformEvent;
    Task ExecuteRuleByIdAsync(Guid ruleId, PlatformEvent @event, CancellationToken ct);
}

public interface IConditionEvaluator
{
    Task<bool> EvaluateAsync(AutomationRule rule, PlatformEvent @event, CancellationToken ct = default);
}

public interface IAutomationActionExecutor
{
    Task ExecuteAsync(AutomationRule rule, PlatformEvent @event, CancellationToken ct = default);
}
