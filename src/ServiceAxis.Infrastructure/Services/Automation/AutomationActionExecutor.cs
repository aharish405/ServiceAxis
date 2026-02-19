using System.Text.Json;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Common.Models;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Services.Automation;

public class AutomationActionExecutor : IAutomationActionExecutor
{
    private readonly ServiceAxisDbContext    _db;
    private readonly IAssignmentService     _assignment;
    private readonly IStateMachineService   _stateMachine;
    private readonly INotificationService   _notification;
    private readonly IActivityService       _activity;
    private readonly ILogger<AutomationActionExecutor> _logger;

    public AutomationActionExecutor(
        ServiceAxisDbContext db,
        IAssignmentService assignment,
        IStateMachineService stateMachine,
        INotificationService notification,
        IActivityService activity,
        ILogger<AutomationActionExecutor> logger)
    {
        _db           = db;
        _assignment   = assignment;
        _stateMachine = stateMachine;
        _notification = notification;
        _activity     = activity;
        _logger       = logger;
    }

    public async Task ExecuteAsync(AutomationRule rule, PlatformEvent @event, CancellationToken ct = default)
    {
        foreach (var action in rule.Actions)
        {
            try
            {
                await ExecuteSingleActionAsync(action, @event, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing automation action {ActionId} for rule {RuleId}", action.Id, rule.Id);
                await _activity.LogActivityAsync(
                    rule.TableId,
                    @event.RecordId,
                    ActivityType.AutomationFailed,
                    $"Automation Action Failed: {action.ActionType}. Error: {ex.Message}",
                    isSystem: true,
                    ct: ct);
            }
        }
    }

    private async Task ExecuteSingleActionAsync(AutomationAction action, PlatformEvent @event, CancellationToken ct)
    {
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(action.ConfigurationJson) ?? new();

        switch (action.ActionType)
        {
            case AutomationActionType.UpdateField:
                if (config.TryGetValue("fieldId", out var fieldIdStr) && Guid.TryParse(fieldIdStr, out var fieldId) && config.TryGetValue("value", out var value))
                {
                    // Update record value
                    var recordValue = _db.RecordValues.FirstOrDefault(v => v.RecordId == @event.RecordId && v.FieldId == fieldId);
                    if (recordValue == null)
                    {
                        _db.RecordValues.Add(new Domain.Entities.Records.RecordValue
                        {
                            RecordId = @event.RecordId,
                            FieldId = fieldId,
                            Value = value
                        });
                    }
                    else
                    {
                        recordValue.Value = value;
                    }
                    await _db.SaveChangesAsync(ct);
                }
                break;

            case AutomationActionType.AssignUser:
                if (config.TryGetValue("userId", out var userId))
                {
                    await _assignment.AssignAsync(@event.RecordId, userId, null, ct);
                }
                break;

            case AutomationActionType.AssignGroup:
                if (config.TryGetValue("groupId", out var groupIdStr) && Guid.TryParse(groupIdStr, out var groupId))
                {
                    await _assignment.AssignAsync(@event.RecordId, null, groupId, ct);
                }
                break;

            case AutomationActionType.ChangeState:
                if (config.TryGetValue("stateId", out var stateIdStr) && Guid.TryParse(stateIdStr, out var stateId))
                {
                    // We need caller roles, for automation we might use a "System" role
                    await _stateMachine.ChangeStateAsync(@event.RecordId, stateId, new[] { "Admin" }, ct);
                }
                break;

            case AutomationActionType.SendNotification:
                if (config.TryGetValue("templateCode", out var templateCode))
                {
                    // Simplified: empty variables and no specific recipients (assumes template handles it or fails gracefully)
                    await _notification.SendAsync(templateCode, new Dictionary<string, string>(), new string[] { }, NotificationChannelType.Email, @event.RecordId, ct);
                }
                break;

            case AutomationActionType.AddComment:
                if (config.TryGetValue("comment", out var comment))
                {
                    await _activity.AddCommentAsync(@event.RecordId, comment, false, ct);
                }
                break;
        }
    }
}
