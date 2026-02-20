using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Workflow.Commands;

public record SyncWorkflowDefinitionCommand(
    Guid DefinitionId,
    List<SyncStepDto> Steps,
    List<SyncTransitionDto> Transitions) : IRequest;

public record SyncStepDto(
    Guid? Id,
    string Code,
    string Name,
    string StepType,
    int Order,
    bool IsInitial,
    bool IsTerminal,
    string? RequiredRole,
    string? Configuration,
    double? X,
    double? Y);

public record SyncTransitionDto(
    Guid? Id,
    string FromStepCode,
    string ToStepCode,
    string TriggerEvent,
    string? Condition,
    int Priority);

public class SyncWorkflowDefinitionHandler : IRequestHandler<SyncWorkflowDefinitionCommand>
{
    private readonly IUnitOfWork _uow;

    public SyncWorkflowDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(SyncWorkflowDefinitionCommand cmd, CancellationToken ct)
    {
        var def = await _uow.Repository<WorkflowDefinition>().GetByIdAsync(cmd.DefinitionId, ct)
            ?? throw new NotFoundException("WorkflowDefinition", cmd.DefinitionId);

        // 1. Sync Steps
        var existingSteps = await _uow.Repository<WorkflowStep>()
            .FindAsync(s => s.DefinitionId == cmd.DefinitionId && s.IsActive, ct);

        var stepsToKeep = new List<Guid>();

        foreach (var sDto in cmd.Steps)
        {
            var step = existingSteps.FirstOrDefault(s => s.Code == sDto.Code.ToUpperInvariant());
            if (step == null)
            {
                step = new WorkflowStep
                {
                    DefinitionId = cmd.DefinitionId,
                    Code = sDto.Code.ToUpperInvariant(),
                    Name = sDto.Name,
                    StepType = sDto.StepType,
                    Order = sDto.Order,
                    IsInitial = sDto.IsInitial,
                    IsTerminal = sDto.IsTerminal,
                    RequiredRole = sDto.RequiredRole,
                    Configuration = sDto.Configuration,
                    X = sDto.X,
                    Y = sDto.Y
                };
                await _uow.Repository<WorkflowStep>().AddAsync(step, ct);
            }
            else
            {
                step.Name = sDto.Name;
                step.StepType = sDto.StepType;
                step.Order = sDto.Order;
                step.IsInitial = sDto.IsInitial;
                step.IsTerminal = sDto.IsTerminal;
                step.RequiredRole = sDto.RequiredRole;
                step.Configuration = sDto.Configuration;
                step.X = sDto.X;
                step.Y = sDto.Y;
                step.UpdatedAt = DateTime.UtcNow;
                _uow.Repository<WorkflowStep>().Update(step);
            }
            
            // We need to save changes to ensure new steps get IDs if we use them later
            await _uow.SaveChangesAsync(ct);
            stepsToKeep.Add(step.Id);
        }

        // Delete steps not in the new list
        foreach (var oldStep in existingSteps)
        {
            if (!stepsToKeep.Contains(oldStep.Id))
                _uow.Repository<WorkflowStep>().SoftDelete(oldStep);
        }

        await _uow.SaveChangesAsync(ct);

        // 2. Sync Transitions
        var allSteps = await _uow.Repository<WorkflowStep>()
            .FindAsync(s => s.DefinitionId == cmd.DefinitionId && s.IsActive, ct);

        var existingTransitions = await _uow.Repository<WorkflowTransition>()
            .FindAsync(t => t.DefinitionId == cmd.DefinitionId && t.IsActive, ct);

        // Easier to clear and recreate for a full sync state
        foreach (var oldTrans in existingTransitions)
            _uow.Repository<WorkflowTransition>().SoftDelete(oldTrans);

        foreach (var tDto in cmd.Transitions)
        {
            var fromStep = allSteps.FirstOrDefault(s => s.Code == tDto.FromStepCode.ToUpperInvariant());
            var toStep = allSteps.FirstOrDefault(s => s.Code == tDto.ToStepCode.ToUpperInvariant());

            if (fromStep != null && toStep != null)
            {
                var transition = new WorkflowTransition
                {
                    DefinitionId = cmd.DefinitionId,
                    FromStepId = fromStep.Id,
                    ToStepId = toStep.Id,
                    TriggerEvent = tDto.TriggerEvent,
                    Condition = tDto.Condition,
                    Priority = tDto.Priority
                };
                await _uow.Repository<WorkflowTransition>().AddAsync(transition, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }
}
