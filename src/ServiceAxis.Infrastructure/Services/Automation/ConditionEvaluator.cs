using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Common.Models;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Services.Automation;

public class ConditionEvaluator : IConditionEvaluator
{
    private readonly ServiceAxisDbContext _db;
    private readonly IMetadataCache       _metadata;

    public ConditionEvaluator(ServiceAxisDbContext db, IMetadataCache metadata)
    {
        _db       = db;
        _metadata = metadata;
    }

    public async Task<bool> EvaluateAsync(AutomationRule rule, PlatformEvent @event, CancellationToken ct = default)
    {
        if (!rule.Conditions.Any()) return true;

        var results = new List<bool>();
        
        // Batch load current values for the record
        var currentValues = await _db.RecordValues
            .Where(v => v.RecordId == @event.RecordId)
            .ToDictionaryAsync(v => v.FieldId, v => v.Value, ct);

        foreach (var condition in rule.Conditions)
        {
            var field = await _metadata.GetFieldAsync(condition.FieldId, ct);
            if (field == null) 
            {
                results.Add(false);
                continue;
            }

            var currentValue = currentValues.TryGetValue(condition.FieldId, out var val) ? val : null;
            var isMatch = EvaluateSingleCondition(condition, field, currentValue, @event);
            results.Add(isMatch);
        }

        // Logic check: Simple flat implementation of OR/AND
        if (rule.Conditions.Any(c => c.LogicalGroup == LogicalGroup.Or))
        {
             return results.Any(r => r);
        }
        
        return results.All(r => r);
    }

    private bool EvaluateSingleCondition(AutomationCondition condition, SysField field, string? currentVal, PlatformEvent @event)
    {
        switch (condition.Operator)
        {
            case AutomationOperator.Equals:
                return string.Equals(currentVal, condition.Value, StringComparison.OrdinalIgnoreCase);
            
            case AutomationOperator.NotEquals:
                return !string.Equals(currentVal, condition.Value, StringComparison.OrdinalIgnoreCase);

            case AutomationOperator.GreaterThan:
                if (decimal.TryParse(currentVal, out var curD) && decimal.TryParse(condition.Value, out var condD))
                    return curD > condD;
                return string.Compare(currentVal, condition.Value, StringComparison.OrdinalIgnoreCase) > 0;

            case AutomationOperator.LessThan:
                if (decimal.TryParse(currentVal, out var curDL) && decimal.TryParse(condition.Value, out var condDL))
                    return curDL < condDL;
                return string.Compare(currentVal, condition.Value, StringComparison.OrdinalIgnoreCase) < 0;

            case AutomationOperator.Contains:
                return currentVal?.Contains(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) ?? false;

            case AutomationOperator.StartsWith:
                return currentVal?.StartsWith(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) ?? false;

            case AutomationOperator.ChangedTo:
                if (@event is RecordUpdatedEvent updatedEvent && updatedEvent.ChangedFields.TryGetValue(field.FieldName, out var changeTo))
                {
                    return string.Equals(changeTo.NewValue, condition.Value, StringComparison.OrdinalIgnoreCase);
                }
                return false;

            case AutomationOperator.ChangedFrom:
                if (@event is RecordUpdatedEvent updatedFromEvent && updatedFromEvent.ChangedFields.TryGetValue(field.FieldName, out var changeFrom))
                {
                    return string.Equals(changeFrom.OldValue, condition.Value, StringComparison.OrdinalIgnoreCase);
                }
                return false;

            default:
                return false;
        }
    }
}
