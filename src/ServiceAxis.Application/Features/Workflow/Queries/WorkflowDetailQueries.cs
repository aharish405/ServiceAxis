using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Application.Features.Workflow.Queries;

// ─── Get Workflow Definition by Id ───────────────────────────────────────────

public record GetWorkflowDefinitionQuery(Guid Id) : IRequest<WorkflowDefinitionDetailDto?>;

public record WorkflowDefinitionDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Category,
    int Version,
    bool IsPublished,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<WorkflowStepDetailDto> Steps,
    IReadOnlyList<WorkflowTransitionDetailDto> Transitions);

public record WorkflowStepDetailDto(
    Guid Id,
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

public record WorkflowTransitionDetailDto(
    Guid Id,
    Guid FromStepId,
    string FromStepCode,
    Guid ToStepId,
    string ToStepCode,
    string TriggerEvent,
    string? Condition,
    int Priority);

public class GetWorkflowDefinitionHandler : IRequestHandler<GetWorkflowDefinitionQuery, WorkflowDefinitionDetailDto?>
{
    private readonly IUnitOfWork _uow;

    public GetWorkflowDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<WorkflowDefinitionDetailDto?> Handle(GetWorkflowDefinitionQuery q, CancellationToken ct)
    {
        var def = await _uow.Repository<WorkflowDefinition>().GetByIdAsync(q.Id, ct);
        if (def == null) return null;

        // Fetch steps and transitions separately — keeps Application layer free of EF Core
        var steps = await _uow.Repository<WorkflowStep>()
            .FindAsync(s => s.DefinitionId == q.Id && s.IsActive, ct);
        var transitions = await _uow.Repository<WorkflowTransition>()
            .FindAsync(t => t.DefinitionId == q.Id && t.IsActive, ct);

        // Build a lookup for step code resolution
        var stepLookup = steps.ToDictionary(s => s.Id, s => s.Code);

        return new WorkflowDefinitionDetailDto(
            def.Id, def.Code, def.Name, def.Description, def.Category,
            def.Version, def.IsPublished, def.IsActive, def.CreatedAt,
            steps.OrderBy(s => s.Order).Select(s => new WorkflowStepDetailDto(
                s.Id, s.Code, s.Name, s.StepType, s.Order,
                s.IsInitial, s.IsTerminal, s.RequiredRole, s.Configuration,
                s.X, s.Y)).ToList(),
            transitions.OrderBy(t => t.Priority).Select(t => new WorkflowTransitionDetailDto(
                t.Id, t.FromStepId,
                stepLookup.GetValueOrDefault(t.FromStepId, ""),
                t.ToStepId,
                stepLookup.GetValueOrDefault(t.ToStepId, ""),
                t.TriggerEvent, t.Condition, t.Priority)).ToList());
    }
}

// ─── Get Steps for a Definition ──────────────────────────────────────────────

public record GetWorkflowStepsQuery(Guid DefinitionId) : IRequest<IReadOnlyList<WorkflowStepDetailDto>>;

public class GetWorkflowStepsHandler : IRequestHandler<GetWorkflowStepsQuery, IReadOnlyList<WorkflowStepDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetWorkflowStepsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<WorkflowStepDetailDto>> Handle(GetWorkflowStepsQuery q, CancellationToken ct)
    {
        var steps = await _uow.Repository<WorkflowStep>()
            .FindAsync(s => s.DefinitionId == q.DefinitionId && s.IsActive, ct);

        return steps.OrderBy(s => s.Order)
            .Select(s => new WorkflowStepDetailDto(s.Id, s.Code, s.Name, s.StepType, s.Order,
                s.IsInitial, s.IsTerminal, s.RequiredRole, s.Configuration,
                s.X, s.Y))
            .ToList();
    }
}

// ─── Get Transitions for a Definition ────────────────────────────────────────

public record GetWorkflowTransitionsQuery(Guid DefinitionId) : IRequest<IReadOnlyList<WorkflowTransitionDetailDto>>;

public class GetWorkflowTransitionsHandler : IRequestHandler<GetWorkflowTransitionsQuery, IReadOnlyList<WorkflowTransitionDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetWorkflowTransitionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<WorkflowTransitionDetailDto>> Handle(GetWorkflowTransitionsQuery q, CancellationToken ct)
    {
        var transitions = await _uow.Repository<WorkflowTransition>()
            .FindAsync(t => t.DefinitionId == q.DefinitionId && t.IsActive, ct);

        // Resolve step codes via a separate lookup
        var stepIds = transitions.SelectMany(t => new[] { t.FromStepId, t.ToStepId }).Distinct().ToList();
        var steps = await _uow.Repository<WorkflowStep>()
            .FindAsync(s => stepIds.Contains(s.Id), ct);
        var stepLookup = steps.ToDictionary(s => s.Id, s => s.Code);

        return transitions.OrderBy(t => t.Priority)
            .Select(t => new WorkflowTransitionDetailDto(
                t.Id, t.FromStepId, stepLookup.GetValueOrDefault(t.FromStepId, ""),
                t.ToStepId, stepLookup.GetValueOrDefault(t.ToStepId, ""),
                t.TriggerEvent, t.Condition, t.Priority))
            .ToList();
    }
}

// ─── Get Workflow Instances ───────────────────────────────────────────────────

public record GetWorkflowInstancesQuery(Guid DefinitionId, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<WorkflowInstanceDto>>;

public record WorkflowInstanceDto(
    Guid Id,
    string ReferenceNumber,
    string? TriggerEntityType,
    string? TriggerEntityId,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    Guid? TenantId);

public class GetWorkflowInstancesHandler : IRequestHandler<GetWorkflowInstancesQuery, PagedResult<WorkflowInstanceDto>>
{
    private readonly IUnitOfWork _uow;

    public GetWorkflowInstancesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<WorkflowInstanceDto>> Handle(GetWorkflowInstancesQuery q, CancellationToken ct)
    {
        var paged = await _uow.Repository<WorkflowInstance>()
            .GetPagedAsync(q.PageNumber, q.PageSize,
                i => i.DefinitionId == q.DefinitionId && i.IsActive, ct);

        return new PagedResult<WorkflowInstanceDto>
        {
            Items = paged.Items.Select(i => new WorkflowInstanceDto(
                i.Id, i.ReferenceNumber, i.TriggerEntityType, i.TriggerEntityId,
                i.Status.ToString(), i.StartedAt, i.CompletedAt, i.TenantId)).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}
