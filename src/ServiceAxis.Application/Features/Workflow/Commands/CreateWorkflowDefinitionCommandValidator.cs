using FluentValidation;

namespace ServiceAxis.Application.Features.Workflow.Commands;

/// <summary>
/// Validates the <see cref="CreateWorkflowDefinitionCommand"/> before the handler executes.
/// </summary>
public sealed class CreateWorkflowDefinitionCommandValidator
    : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Workflow code is required.")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Code must be uppercase alphanumeric (A-Z, 0-9, -, _).");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workflow name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.");
    }
}
