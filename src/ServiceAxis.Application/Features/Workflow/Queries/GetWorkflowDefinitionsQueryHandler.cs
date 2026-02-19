using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Application.Features.Workflow.Queries;

/// <summary>
/// Handles <see cref="GetWorkflowDefinitionsQuery"/>.
/// Returns a paged list of workflow definitions, optionally filtered by category.
/// </summary>
public sealed class GetWorkflowDefinitionsQueryHandler
    : IRequestHandler<GetWorkflowDefinitionsQuery, PagedResult<WorkflowDefinitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWorkflowDefinitionsQueryHandler(IUnitOfWork unitOfWork) =>
        _unitOfWork = unitOfWork;

    public async Task<PagedResult<WorkflowDefinitionDto>> Handle(
        GetWorkflowDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<WorkflowDefinition>();

        var paged = await repo.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate: w => w.IsActive &&
                            (request.Category == null || w.Category == request.Category),
            cancellationToken: cancellationToken);

        var dtos = paged.Items.Select(w => new WorkflowDefinitionDto(
            w.Id, w.Code, w.Name, w.Description, w.Category,
            w.Version, w.IsPublished, w.IsActive, w.CreatedAt));

        return new PagedResult<WorkflowDefinitionDto>
        {
            Items      = dtos.ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}
