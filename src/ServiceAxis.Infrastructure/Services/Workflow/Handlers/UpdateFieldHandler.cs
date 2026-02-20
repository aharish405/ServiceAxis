using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Application.Contracts.Infrastructure;
using System.Text.Json;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Infrastructure.Services.Workflow.Handlers;

public class UpdateFieldHandler : IWorkflowStepHandler
{
    private readonly IRecordValueRepository _values;
    private readonly IMetadataCache _cache;

    public UpdateFieldHandler(IRecordValueRepository values, IMetadataCache cache)
    {
        _values = values;
        _cache = cache;
    }

    public string StepType => "UpdateField";

    public async Task<StepResult> ExecuteAsync(WorkflowInstance instance, WorkflowStep step, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(step.Configuration)) return StepResult.Failed("Missing configuration");

        try
        {
            var config = JsonDocument.Parse(step.Configuration);
            var fieldName = config.RootElement.GetProperty("fieldName").GetString();
            var value = config.RootElement.GetProperty("value").GetString();

            if (string.IsNullOrEmpty(fieldName) || instance.TriggerEntityId == null) 
                return StepResult.Failed("Invalid fieldName or missing target entity");

            var table = await _cache.GetTableAsync(instance.TriggerEntityType!, ct);
            var field = table?.Fields.FirstOrDefault(f => f.FieldName == fieldName);

            if (field == null) return StepResult.Failed($"Field '{fieldName}' not found on table '{instance.TriggerEntityType}'");

            await _values.UpsertValueAsync(Guid.Parse(instance.TriggerEntityId), field.Id, value, ct);

            return StepResult.Completed("Success");
        }
        catch (Exception ex)
        {
            return StepResult.Failed(ex.Message);
        }
    }
}
