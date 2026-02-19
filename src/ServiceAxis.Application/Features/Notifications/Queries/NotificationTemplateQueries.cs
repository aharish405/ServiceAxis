using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Notifications;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Notifications.Queries;

// ─── List Notification Templates ──────────────────────────────────────────────

public record ListNotificationTemplatesQuery(
    string? TableName = null,
    string? TriggerEvent = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationTemplateDto>>;

public record NotificationTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Subject,
    string TriggerEvent,
    string? TableName,
    bool IsSystemTemplate,
    bool IsActive,
    DateTime CreatedAt);

public class ListNotificationTemplatesHandler
    : IRequestHandler<ListNotificationTemplatesQuery, PagedResult<NotificationTemplateDto>>
{
    private readonly IUnitOfWork _uow;

    public ListNotificationTemplatesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<NotificationTemplateDto>> Handle(
        ListNotificationTemplatesQuery q, CancellationToken ct)
    {
        var paged = await _uow.Repository<NotificationTemplate>()
            .GetPagedAsync(q.Page, q.PageSize,
                t => t.IsActive &&
                     (q.TableName == null || t.TableName == q.TableName) &&
                     (q.TriggerEvent == null || t.TriggerEvent.ToString() == q.TriggerEvent),
                ct);

        return new PagedResult<NotificationTemplateDto>
        {
            Items = paged.Items.Select(t => new NotificationTemplateDto(
                t.Id, t.Code, t.Name, t.Description, t.Subject,
                t.TriggerEvent.ToString(), t.TableName, t.IsSystemTemplate,
                t.IsActive, t.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}

// ─── Get Single Template ──────────────────────────────────────────────────────

public record GetNotificationTemplateQuery(Guid Id)
    : IRequest<NotificationTemplateDetailDto?>;

public record NotificationTemplateDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Subject,
    string Body,
    string TriggerEvent,
    string? TableName,
    bool IsSystemTemplate,
    bool IsActive,
    DateTime CreatedAt);

public class GetNotificationTemplateHandler
    : IRequestHandler<GetNotificationTemplateQuery, NotificationTemplateDetailDto?>
{
    private readonly IUnitOfWork _uow;

    public GetNotificationTemplateHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<NotificationTemplateDetailDto?> Handle(
        GetNotificationTemplateQuery q, CancellationToken ct)
    {
        var t = await _uow.Repository<NotificationTemplate>().GetByIdAsync(q.Id, ct);
        if (t is null) return null;

        return new NotificationTemplateDetailDto(
            t.Id, t.Code, t.Name, t.Description, t.Subject, t.Body,
            t.TriggerEvent.ToString(), t.TableName, t.IsSystemTemplate,
            t.IsActive, t.CreatedAt);
    }
}
