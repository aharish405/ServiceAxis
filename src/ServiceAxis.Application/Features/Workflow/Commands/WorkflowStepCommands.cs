using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Workflow.Commands;

// ─── Add Step ────────────────────────────────────────────────────────────────

public record AddWorkflowStepCommand(
    Guid DefinitionId,
    string Code,
    string Name,
    string StepType,
    int Order,
    bool IsInitial,
    bool IsTerminal,
    string? RequiredRole,
    string? Configuration) : IRequest<WorkflowStepDto>;

public record WorkflowStepDto(
    Guid Id,
    Guid DefinitionId,
    string Code,
    string Name,
    string StepType,
    int Order,
    bool IsInitial,
    bool IsTerminal,
    string? RequiredRole);

public class AddWorkflowStepHandler : IRequestHandler<AddWorkflowStepCommand, WorkflowStepDto>
{
    private readonly IUnitOfWork _uow;

    public AddWorkflowStepHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<WorkflowStepDto> Handle(AddWorkflowStepCommand cmd, CancellationToken ct)
    {
        // Verify definition exists
        var def = await _uow.Repository<WorkflowDefinition>().GetByIdAsync(cmd.DefinitionId, ct)
            ?? throw new NotFoundException("WorkflowDefinition", cmd.DefinitionId);

        // Only one initial step allowed
        if (cmd.IsInitial)
        {
            var hasInitial = await _uow.Repository<WorkflowStep>()
                .ExistsAsync(s => s.DefinitionId == cmd.DefinitionId && s.IsInitial && s.IsActive, ct);
            if (hasInitial)
                throw new ConflictException("A workflow definition can only have one initial step.");
        }

        // Unique code within definition
        var codeConflict = await _uow.Repository<WorkflowStep>()
            .ExistsAsync(s => s.DefinitionId == cmd.DefinitionId && s.Code == cmd.Code && s.IsActive, ct);
        if (codeConflict)
            throw new ConflictException($"Step with code '{cmd.Code}' already exists in this workflow.");

        var step = new WorkflowStep
        {
            DefinitionId  = cmd.DefinitionId,
            Code          = cmd.Code.ToUpperInvariant(),
            Name          = cmd.Name,
            StepType      = cmd.StepType,
            Order         = cmd.Order,
            IsInitial     = cmd.IsInitial,
            IsTerminal    = cmd.IsTerminal,
            RequiredRole  = cmd.RequiredRole,
            Configuration = cmd.Configuration
        };

        await _uow.Repository<WorkflowStep>().AddAsync(step, ct);
        await _uow.SaveChangesAsync(ct);

        return new WorkflowStepDto(step.Id, step.DefinitionId, step.Code, step.Name,
            step.StepType, step.Order, step.IsInitial, step.IsTerminal, step.RequiredRole);
    }
}

// ─── Update Step ─────────────────────────────────────────────────────────────

public record UpdateWorkflowStepCommand(
    Guid StepId,
    string Name,
    string StepType,
    int Order,
    bool IsTerminal,
    string? RequiredRole,
    string? Configuration) : IRequest<WorkflowStepDto>;

public class UpdateWorkflowStepHandler : IRequestHandler<UpdateWorkflowStepCommand, WorkflowStepDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateWorkflowStepHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<WorkflowStepDto> Handle(UpdateWorkflowStepCommand cmd, CancellationToken ct)
    {
        var step = await _uow.Repository<WorkflowStep>().GetByIdAsync(cmd.StepId, ct)
            ?? throw new NotFoundException("WorkflowStep", cmd.StepId);

        step.Name          = cmd.Name;
        step.StepType      = cmd.StepType;
        step.Order         = cmd.Order;
        step.IsTerminal    = cmd.IsTerminal;
        step.RequiredRole  = cmd.RequiredRole;
        step.Configuration = cmd.Configuration;
        step.UpdatedAt     = DateTime.UtcNow;

        _uow.Repository<WorkflowStep>().Update(step);
        await _uow.SaveChangesAsync(ct);

        return new WorkflowStepDto(step.Id, step.DefinitionId, step.Code, step.Name,
            step.StepType, step.Order, step.IsInitial, step.IsTerminal, step.RequiredRole);
    }
}

// ─── Delete Step ─────────────────────────────────────────────────────────────

public record DeleteWorkflowStepCommand(Guid StepId) : IRequest;

public class DeleteWorkflowStepHandler : IRequestHandler<DeleteWorkflowStepCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteWorkflowStepHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(DeleteWorkflowStepCommand cmd, CancellationToken ct)
    {
        var step = await _uow.Repository<WorkflowStep>().GetByIdAsync(cmd.StepId, ct)
            ?? throw new NotFoundException("WorkflowStep", cmd.StepId);

        _uow.Repository<WorkflowStep>().SoftDelete(step);
        await _uow.SaveChangesAsync(ct);
    }
}

// ─── Add Transition ──────────────────────────────────────────────────────────

public record AddWorkflowTransitionCommand(
    Guid DefinitionId,
    Guid FromStepId,
    Guid ToStepId,
    string TriggerEvent,
    string? Condition,
    int Priority) : IRequest<WorkflowTransitionDto>;

public record WorkflowTransitionDto(
    Guid Id,
    Guid DefinitionId,
    Guid FromStepId,
    Guid ToStepId,
    string TriggerEvent,
    string? Condition,
    int Priority);

public class AddWorkflowTransitionHandler : IRequestHandler<AddWorkflowTransitionCommand, WorkflowTransitionDto>
{
    private readonly IUnitOfWork _uow;

    public AddWorkflowTransitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<WorkflowTransitionDto> Handle(AddWorkflowTransitionCommand cmd, CancellationToken ct)
    {
        // Validate steps belong to the definition
        var fromStep = await _uow.Repository<WorkflowStep>().GetByIdAsync(cmd.FromStepId, ct)
            ?? throw new NotFoundException("WorkflowStep (From)", cmd.FromStepId);
        var toStep = await _uow.Repository<WorkflowStep>().GetByIdAsync(cmd.ToStepId, ct)
            ?? throw new NotFoundException("WorkflowStep (To)", cmd.ToStepId);

        if (fromStep.DefinitionId != cmd.DefinitionId || toStep.DefinitionId != cmd.DefinitionId)
            throw new BusinessException("Steps must belong to the specified workflow definition.");

        var transition = new WorkflowTransition
        {
            DefinitionId  = cmd.DefinitionId,
            FromStepId    = cmd.FromStepId,
            ToStepId      = cmd.ToStepId,
            TriggerEvent  = cmd.TriggerEvent,
            Condition     = cmd.Condition,
            Priority      = cmd.Priority
        };

        await _uow.Repository<WorkflowTransition>().AddAsync(transition, ct);
        await _uow.SaveChangesAsync(ct);

        return new WorkflowTransitionDto(transition.Id, transition.DefinitionId,
            transition.FromStepId, transition.ToStepId, transition.TriggerEvent,
            transition.Condition, transition.Priority);
    }
}

// ─── Delete Transition ───────────────────────────────────────────────────────

public record DeleteWorkflowTransitionCommand(Guid TransitionId) : IRequest;

public class DeleteWorkflowTransitionHandler : IRequestHandler<DeleteWorkflowTransitionCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteWorkflowTransitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(DeleteWorkflowTransitionCommand cmd, CancellationToken ct)
    {
        var transition = await _uow.Repository<WorkflowTransition>().GetByIdAsync(cmd.TransitionId, ct)
            ?? throw new NotFoundException("WorkflowTransition", cmd.TransitionId);

        _uow.Repository<WorkflowTransition>().SoftDelete(transition);
        await _uow.SaveChangesAsync(ct);
    }
}
