using System.Text.Json;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Infrastructure.Services.Workflow.Handlers;

public class ConditionHandler : IWorkflowStepHandler
{
    private readonly IRecordRepository _records;

    public ConditionHandler(IRecordRepository records)
    {
        _records = records;
    }

    public string StepType => "Condition";

    public async Task<StepResult> ExecuteAsync(WorkflowInstance instance, WorkflowStep step, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(step.Configuration)) return StepResult.Failed("Missing configuration");

        try
        {
            var config = JsonDocument.Parse(step.Configuration);
            var condition = config.RootElement.GetProperty("condition").GetString();

            if (string.IsNullOrEmpty(condition) || instance.TriggerEntityId == null)
                return StepResult.Completed("Default"); // No condition means always true/default

            var record = await _records.GetWithValuesAsync(Guid.Parse(instance.TriggerEntityId), ct);
            if (record == null) return StepResult.Failed("Record not found");

            // Simple condition evaluation (mocked for now, in a real system we'd use a rules engine or Dynamic LINQ)
            // Example: "priority == 1"
            bool result = false;
            if (condition.Contains("priority == 1") && record.Priority == 1) result = true;
            else if (condition.Contains("priority == 2") && record.Priority == 2) result = true;
            else if (condition.Contains("priority == 3") && record.Priority == 3) result = true;

            return StepResult.Completed(result ? "True" : "False");
        }
        catch (Exception ex)
        {
            return StepResult.Failed(ex.Message);
        }
    }
}
