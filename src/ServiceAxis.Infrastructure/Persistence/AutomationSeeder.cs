using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Persistence;

public static class AutomationSeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db)
    {
        var incidentTable = await db.SysTables.FirstOrDefaultAsync(t => t.Name == "incident");
        if (incidentTable == null) return; // Wait for metadata to be seeded

        var ruleExists = await db.AutomationRules.AnyAsync(r => r.Name == "High Priority Auto Assignment" && r.TableId == incidentTable.Id);
        
        if (!ruleExists)
        {
            var priorityField = await db.SysFields.FirstOrDefaultAsync(f => f.TableId == incidentTable.Id && f.FieldName == "priority");
            if (priorityField == null) return;

            var rule = new AutomationRule
            {
                Name = "High Priority Auto Assignment",
                Description = "Automatically assigns high priority (1 or 2) incidents to the Network group.",
                TableId = incidentTable.Id,
                ExecutionMode = AutomationExecutionMode.Synchronous,
                StopProcessingOnMatch = true,
                IsActive = true
            };

            db.AutomationRules.Add(rule);

            // Trigger on Create or Update
            db.AutomationTriggers.AddRange(
                new AutomationTrigger { Rule = rule, EventType = AutomationEventType.RecordCreated },
                new AutomationTrigger { Rule = rule, EventType = AutomationEventType.RecordUpdated }
            );

            // Condition: Priority <= 2 (1: Critical, 2: High)
            db.AutomationConditions.Add(
                new AutomationCondition
                {
                    Rule = rule,
                    FieldId = priorityField.Id,
                    Operator = AutomationOperator.LessThan,
                    Value = "3",
                    LogicalGroup = LogicalGroup.And
                }
            );

            // Action: Assign to Group "Network"
            // Get Network group to get ID
            var networkGroup = await db.AssignmentGroups.FirstOrDefaultAsync(g => g.Name == "Network");
            if (networkGroup != null)
            {
                var actionConfig = new Dictionary<string, string>
                {
                    { "groupId", networkGroup.Id.ToString() }
                };

                db.AutomationActions.Add(
                    new AutomationAction
                    {
                        Rule = rule,
                        ActionType = AutomationActionType.AssignGroup,
                        ConfigurationJson = JsonSerializer.Serialize(actionConfig)
                    }
                );
            }

            await db.SaveChangesAsync();
        }
    }
}
