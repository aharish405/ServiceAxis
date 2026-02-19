using MediatR;

namespace ServiceAxis.Application.Features.Workflow.Commands;

/// <summary>
/// Command to create a new workflow definition (blueprint).
/// </summary>
public record CreateWorkflowDefinitionCommand(
    string Code,
    string Name,
    string? Description,
    string Category,
    Guid? TenantId) : IRequest<CreateWorkflowDefinitionResult>;

/// <summary>
/// Result returned after creating a workflow definition.
/// </summary>
public record CreateWorkflowDefinitionResult(
    Guid Id,
    string Code,
    string Name,
    int Version,
    DateTime CreatedAt);
