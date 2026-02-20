using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.Infrastructure.Persistence;

public static class DynamicUISeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db)
    {
        if (await db.UiPolicies.AnyAsync()) return;

        var incidentTable = await db.SysTables.FirstOrDefaultAsync(t => t.Name == "incident");
        if (incidentTable == null) return;

        var priorityField = await db.SysFields.FirstOrDefaultAsync(f => f.TableId == incidentTable.Id && f.FieldName == "priority");
        var titleField = await db.SysFields.FirstOrDefaultAsync(f => f.TableId == incidentTable.Id && f.FieldName == "title");

        if (priorityField != null && titleField != null)
        {
            // Seed a UI Policy: If Priority = 1, Show and Mandatory Group
            var highPriorityPolicy = new UiPolicy
            {
                Name = "High Priority Group Enforcement",
                Description = "If an incident is marked high priority, Assignment Group is mandatory.",
                TableId = incidentTable.Id,
                IsActive = true,
                ExecutionOrder = 100,
                FormContext = FormContextType.All,
                Conditions = [
                    new UiPolicyCondition
                    {
                        FieldId = priorityField.Id,
                        Operator = UiPolicyOperator.Equals,
                        Value = "1"
                    }
                ],
                Actions = [
                    new UiPolicyAction
                    {
                        TargetFieldId = titleField.Id,
                        ActionType = UiPolicyActionType.MakeMandatory
                    }
                ]
            };
            db.UiPolicies.Add(highPriorityPolicy);
            
            // Seed a Client Script
            var alertScript = new ClientScript
            {
                Name = "Warn On Critical",
                Description = "Alerts the user when critical priority is chosen.",
                TableId = incidentTable.Id,
                EventType = ClientScriptEventType.OnChange,
                TriggerFieldId = priorityField.Id,
                ScriptCode = "function invoke(form) { if (form.getValue('priority') === '1') { alert('Warning: Escalate Critical Priority Immediately.'); } }",
                ExecutionOrder = 300
            };
            db.ClientScripts.Add(alertScript);

            await db.SaveChangesAsync();
        }
    }
}
