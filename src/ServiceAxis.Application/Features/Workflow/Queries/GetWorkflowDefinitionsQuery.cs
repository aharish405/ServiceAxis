using MediatR;
using ServiceAxis.Domain.Common;

namespace ServiceAxis.Application.Features.Workflow.Queries;

/// <summary>
/// Query to retrieve a paged list of workflow definitions.
/// </summary>
public record GetWorkflowDefinitionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Category = null) : IRequest<PagedResult<WorkflowDefinitionDto>>;

/// <summary>
/// DTO returned to API callers.
/// </summary>
public record WorkflowDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Category,
    int Version,
    bool IsPublished,
    bool IsActive,
    DateTime CreatedAt);
