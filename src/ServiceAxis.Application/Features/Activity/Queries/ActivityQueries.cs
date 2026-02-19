using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Common;

namespace ServiceAxis.Application.Features.Activity.Queries;

public record GetActivityTimelineQuery(
    string TableName,
    Guid RecordId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ActivityTimelineDto>>;

public class GetActivityTimelineHandler : IRequestHandler<GetActivityTimelineQuery, PagedResult<ActivityTimelineDto>>
{
    private readonly IActivityService _activity;

    public GetActivityTimelineHandler(IActivityService activity)
    {
        _activity = activity;
    }

    public async Task<PagedResult<ActivityTimelineDto>> Handle(GetActivityTimelineQuery request, CancellationToken ct)
    {
        return await _activity.GetTimelineAsync(request.RecordId, request.Page, request.PageSize, ct);
    }
}
