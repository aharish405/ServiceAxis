using System.Text.Json;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskStatus = ServiceAxis.Domain.Entities.Workflow.TaskStatus;

namespace ServiceAxis.Infrastructure.Services.Workflow.Handlers;

public class ApprovalHandler : IWorkflowStepHandler
{
    private readonly ServiceAxisDbContext _db;

    public ApprovalHandler(ServiceAxisDbContext db)
    {
        _db = db;
    }

    public string StepType => "Approval";

    public async Task<StepResult> ExecuteAsync(WorkflowInstance instance, WorkflowStep step, CancellationToken ct)
    {
        // Check if an existing task for this step is already completed
        var existingCompletedTask = await _db.WorkflowTasks
            .FirstOrDefaultAsync(t => t.InstanceId == instance.Id && t.StepId == step.Id && t.Status == TaskStatus.Completed, ct);

        if (existingCompletedTask != null)
        {
            // If completed, we decide the transition based on the resolution
            return StepResult.Completed(existingCompletedTask.ResolutionNotes == "Approved" ? "Approved" : "Rejected");
        }

        // Check if a task is already pending
        var pendingTask = await _db.WorkflowTasks
            .FirstOrDefaultAsync(t => t.InstanceId == instance.Id && t.StepId == step.Id && t.Status == TaskStatus.New, ct);

        if (pendingTask != null)
        {
            return StepResult.Waiting();
        }

        // Create new Approval Task
        var task = new WorkflowTask
        {
            InstanceId = instance.Id,
            StepId = step.Id,
            Title = $"Approval Required: {instance.ReferenceNumber}",
            Description = $"Please approve or reject the request for {instance.TriggerEntityType} {instance.TriggerEntityId}",
            Status = TaskStatus.New,
            AssignedToUserId = step.RequiredRole == "Manager" ? "MANAGER_ID_PLACEHOLDER" : null, // Logic for finding real manager would go here
            AssignedToGroupId = string.IsNullOrEmpty(step.RequiredRole) ? null : null, // Logic for group assignment
            TenantId = instance.TenantId
        };

        _db.WorkflowTasks.Add(task);
        // We don't SaveChanges here, the Engine will do it.

        return StepResult.Waiting();
    }
}
