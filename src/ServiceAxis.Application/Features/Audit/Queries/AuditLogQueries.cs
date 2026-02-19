using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Features.Audit.Queries;

// ─── List Audit Logs ──────────────────────────────────────────────────────────

public record ListAuditLogsQuery(
    string? Module      = null,
    string? EntityType  = null,
    string? EntityId    = null,
    string? Action      = null,
    DateTime? From      = null,
    DateTime? To        = null,
    int Page            = 1,
    int PageSize        = 50) : IRequest<PagedResult<AuditLogDto>>;

public record AuditLogDto(
    Guid Id,
    string Module,
    string Action,
    string EntityType,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? CorrelationId,
    string? IpAddress,
    Guid? UserId,
    DateTime CreatedAt);

public class ListAuditLogsHandler : IRequestHandler<ListAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IUnitOfWork _uow;

    public ListAuditLogsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<AuditLogDto>> Handle(ListAuditLogsQuery q, CancellationToken ct)
    {
        var paged = await _uow.Repository<AuditLog>().GetPagedAsync(
            q.Page, q.PageSize,
            a =>
                (q.Module     == null || a.Module     == q.Module) &&
                (q.EntityType == null || a.EntityType == q.EntityType) &&
                (q.EntityId   == null || a.EntityId   == q.EntityId) &&
                (q.Action     == null || a.Action.ToString() == q.Action) &&
                (q.From       == null || a.CreatedAt >= q.From) &&
                (q.To         == null || a.CreatedAt <= q.To),
            ct);

        return new PagedResult<AuditLogDto>
        {
            Items = paged.Items.Select(a => new AuditLogDto(
                a.Id, a.Module, a.Action.ToString(), a.EntityType,
                a.EntityId, a.OldValues, a.NewValues,
                a.CorrelationId, a.IpAddress, a.UserId, a.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}
