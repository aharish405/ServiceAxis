using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Notifications;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Notifications.Commands;

// ─── Create Notification Template ────────────────────────────────────────────

public record CreateNotificationTemplateCommand(
    string Code,
    string Name,
    string? Description,
    string Subject,
    string Body,
    string TriggerEvent,
    string? TableName,
    Guid? TenantId) : IRequest<Guid>;

public class CreateNotificationTemplateHandler
    : IRequestHandler<CreateNotificationTemplateCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateNotificationTemplateHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateNotificationTemplateCommand cmd, CancellationToken ct)
    {
        // Code uniqueness check
        var conflict = await _uow.Repository<NotificationTemplate>()
            .ExistsAsync(t => t.Code == cmd.Code && t.IsActive, ct);
        if (conflict)
            throw new ConflictException($"A notification template with code '{cmd.Code}' already exists.");

        if (!Enum.TryParse<NotificationTriggerEvent>(cmd.TriggerEvent, true, out var triggerEvent))
            throw new BusinessException($"Unknown trigger event '{cmd.TriggerEvent}'.");

        var template = new NotificationTemplate
        {
            Code         = cmd.Code.ToUpperInvariant(),
            Name         = cmd.Name,
            Description  = cmd.Description,
            Subject      = cmd.Subject,
            Body         = cmd.Body,
            TriggerEvent = triggerEvent,
            TableName    = cmd.TableName?.ToLowerInvariant(),
            TenantId     = cmd.TenantId
        };

        await _uow.Repository<NotificationTemplate>().AddAsync(template, ct);
        await _uow.SaveChangesAsync(ct);
        return template.Id;
    }
}

// ─── Update Notification Template ────────────────────────────────────────────

public record UpdateNotificationTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    string Subject,
    string Body,
    bool IsActive) : IRequest;

public class UpdateNotificationTemplateHandler
    : IRequestHandler<UpdateNotificationTemplateCommand>
{
    private readonly IUnitOfWork _uow;

    public UpdateNotificationTemplateHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(UpdateNotificationTemplateCommand cmd, CancellationToken ct)
    {
        var template = await _uow.Repository<NotificationTemplate>().GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("NotificationTemplate", cmd.Id);

        if (template.IsSystemTemplate)
            throw new ForbiddenException("System notification templates cannot be modified.");

        template.Name        = cmd.Name;
        template.Description = cmd.Description;
        template.Subject     = cmd.Subject;
        template.Body        = cmd.Body;
        template.IsActive    = cmd.IsActive;
        template.UpdatedAt   = DateTime.UtcNow;

        _uow.Repository<NotificationTemplate>().Update(template);
        await _uow.SaveChangesAsync(ct);
    }
}

// ─── Delete Notification Template ────────────────────────────────────────────

public record DeleteNotificationTemplateCommand(Guid Id) : IRequest;

public class DeleteNotificationTemplateHandler
    : IRequestHandler<DeleteNotificationTemplateCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteNotificationTemplateHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(DeleteNotificationTemplateCommand cmd, CancellationToken ct)
    {
        var template = await _uow.Repository<NotificationTemplate>().GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("NotificationTemplate", cmd.Id);

        if (template.IsSystemTemplate)
            throw new ForbiddenException("System notification templates cannot be deleted.");

        _uow.Repository<NotificationTemplate>().SoftDelete(template);
        await _uow.SaveChangesAsync(ct);
    }
}
