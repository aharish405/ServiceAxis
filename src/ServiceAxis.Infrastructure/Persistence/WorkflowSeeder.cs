using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Persistence;

public static class WorkflowSeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db)
    {
        if (await db.WorkflowDefinitions.AnyAsync()) return;

        // 1. Incident Approval Workflow
        var incidentApproval = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Code = "incident-approval",
            Name = "Standard Incident Approval",
            Description = "Approval process for high priority incidents.",
            Category = "ITSM",
            Version = 1,
            IsPublished = true
        };

        db.WorkflowDefinitions.Add(incidentApproval);

        var startStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            DefinitionId = incidentApproval.Id,
            Code = "start",
            Name = "New Incident Created",
            StepType = "UpdateField",
            Order = 10,
            IsInitial = true,
            Configuration = "{\"field\": \"Status\", \"value\": \"Pending Approval\"}"
        };

        var managerStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            DefinitionId = incidentApproval.Id,
            Code = "manager_approval",
            Name = "Manager Approval",
            StepType = "Approval",
            Order = 20,
            RequiredRole = "Manager",
            Configuration = "{\"days_to_complete\": 2}"
        };

        var endStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            DefinitionId = incidentApproval.Id,
            Code = "resolved",
            Name = "Approved & Active",
            StepType = "UpdateField",
            Order = 30,
            IsTerminal = true,
            Configuration = "{\"field\": \"Status\", \"value\": \"Active\"}"
        };

        db.WorkflowSteps.AddRange(startStep, managerStep, endStep);

        db.WorkflowTransitions.Add(new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            DefinitionId = incidentApproval.Id,
            FromStepId = startStep.Id,
            ToStepId = managerStep.Id,
            TriggerEvent = "Auto",
            Priority = 10
        });

        db.WorkflowTransitions.Add(new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            DefinitionId = incidentApproval.Id,
            FromStepId = managerStep.Id,
            ToStepId = endStep.Id,
            TriggerEvent = "Approved",
            Priority = 10
        });

        await db.SaveChangesAsync();
    }
}
