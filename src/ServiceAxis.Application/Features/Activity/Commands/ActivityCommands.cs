using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.Application.Features.Activity.Commands;

public record AddCommentCommand(
    string TableName,
    Guid RecordId,
    string CommentText,
    bool IsInternal) : IRequest<bool>;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, bool>
{
    private readonly IActivityService _activity;

    public AddCommentHandler(IActivityService activity)
    {
        _activity = activity;
    }

    public async Task<bool> Handle(AddCommentCommand request, CancellationToken ct)
    {
        await _activity.AddCommentAsync(request.RecordId, request.CommentText, request.IsInternal, ct);
        return true;
    }
}
