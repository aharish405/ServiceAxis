using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Infrastructure.Services.Workflow.Handlers;

public interface IWorkflowStepHandler
{
    string StepType { get; }
    Task<StepResult> ExecuteAsync(WorkflowInstance instance, WorkflowStep step, CancellationToken ct);
}

public class StepResult
{
    public bool IsCompleted { get; set; }
    public string? TriggerEvent { get; set; }
    public string? ErrorMessage { get; set; }

    public static StepResult Completed(string? triggerEvent = null) => new() { IsCompleted = true, TriggerEvent = triggerEvent };
    public static StepResult Waiting() => new() { IsCompleted = false };
    public static StepResult Failed(string error) => new() { IsCompleted = true, ErrorMessage = error };
}
