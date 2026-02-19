using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Workflow.Commands;

/// <summary>
/// Handles <see cref="CreateWorkflowDefinitionCommand"/>.
/// Creates and persists a new workflow definition.
/// </summary>
public sealed class CreateWorkflowDefinitionCommandHandler
    : IRequestHandler<CreateWorkflowDefinitionCommand, CreateWorkflowDefinitionResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkflowDefinitionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateWorkflowDefinitionResult> Handle(
        CreateWorkflowDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<WorkflowDefinition>();

        // Enforce code uniqueness
        bool exists = await repo.ExistsAsync(
            w => w.Code == request.Code && w.IsActive,
            cancellationToken);

        if (exists)
            throw new ConflictException($"A workflow definition with code '{request.Code}' already exists.");

        var definition = new WorkflowDefinition
        {
            Code        = request.Code.ToUpperInvariant(),
            Name        = request.Name,
            Description = request.Description,
            Category    = request.Category,
            TenantId    = request.TenantId,
            Version     = 1,
            IsPublished = false
        };

        await repo.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateWorkflowDefinitionResult(
            definition.Id,
            definition.Code,
            definition.Name,
            definition.Version,
            definition.CreatedAt);
    }
}
