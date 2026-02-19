using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Application.Features.Platform.Queries;

// ─── Platform Dashboard Stats ─────────────────────────────────────────────────

public record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;

public record PlatformStatsDto(
    int TotalRecords,
    int ActiveWorkflows,
    int ActiveSlaInstances,
    int BreachedSlaInstances,
    int OpenWorkflowInstances,
    DateTime GeneratedAt);

public class GetPlatformStatsHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly ServiceAxisDbContext _db;

    public GetPlatformStatsHandler(ServiceAxisDbContext db) => _db = db;

    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery q, CancellationToken ct)
    {
        var totalRecords = await _db.PlatformRecords.CountAsync(r => r.IsActive, ct);

        var activeWorkflows = await _db.WorkflowDefinitions
            .CountAsync(w => w.IsPublished && w.IsActive, ct);

        var activeSla = await _db.SlaInstances
            .CountAsync(s => s.Status == SlaStatus.Active && s.IsActive, ct);

        var breachedSla = await _db.SlaInstances
            .CountAsync(s => s.Status == SlaStatus.Breached && s.IsActive, ct);

        var openInstances = await _db.WorkflowInstances
            .CountAsync(i => i.Status == WorkflowStatus.Active && i.IsActive, ct);

        return new PlatformStatsDto(
            totalRecords,
            activeWorkflows,
            activeSla,
            breachedSla,
            openInstances,
            DateTime.UtcNow);
    }
}
