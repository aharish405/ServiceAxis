using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Activity;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Application.Common.Models;

namespace ServiceAxis.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly ServiceAxisDbContext _db;
    private readonly IPermissionService _permission;
    private readonly IPlatformEventPublisher _events;

    public ActivityService(
        ServiceAxisDbContext db, 
        IPermissionService permission,
        IPlatformEventPublisher events)
    {
        _db = db;
        _permission = permission;
        _events = events;
    }

    public async Task LogActivityAsync(
        Guid tableId, 
        Guid recordId, 
        ActivityType type, 
        string? message, 
        bool isSystem = true,
        IEnumerable<(string FieldName, Guid? FieldId, string? OldValue, string? NewValue)>? changes = null,
        CancellationToken ct = default)
    {
        var activity = new Activity
        {
            TableId = tableId,
            RecordId = recordId,
            Type = type,
            Message = message,
            IsSystemGenerated = isSystem,
            CreatedAt = DateTime.UtcNow
        };

        _db.Activities.Add(activity);

        if (changes != null)
        {
            foreach (var change in changes)
            {
                _db.FieldChanges.Add(new FieldChange
                {
                    ActivityId = activity.Id,
                    FieldId = change.FieldId,
                    FieldName = change.FieldName,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue
                });
            }
        }

        // Note: We don't save changes here. We assume this is called within a Unit of Work context.
        // Wait, the requirement says "Whenever RecordValue changes, automatically generate...".
        // If it's called from a service, the service will call SaveChanges.
        await Task.CompletedTask;
    }

    public async Task AddCommentAsync(Guid recordId, string text, bool isInternal, CancellationToken ct = default)
    {
        // Find the table ID for this record
        var record = await _db.PlatformRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == recordId, ct);
        if (record == null) return;

        var activity = new Activity
        {
            TableId = record.TableId,
            RecordId = recordId,
            Type = isInternal ? ActivityType.WorkNoteAdded : ActivityType.CommentAdded,
            IsSystemGenerated = false
        };

        _db.Activities.Add(activity);
        
        _db.Comments.Add(new Comment
        {
            ActivityId = activity.Id,
            RecordId = recordId,
            CommentText = text,
            IsInternal = isInternal
        });

        await _db.SaveChangesAsync(ct);

        // Publish Event
        var tableName = await _db.SysTables.Where(t => t.Id == record.TableId).Select(t => t.Name).FirstOrDefaultAsync(ct) ?? "";
        await _events.PublishAsync(new CommentAddedEvent {
            RecordId = recordId,
            TableId = record.TableId,
            TableName = tableName,
            TenantId = record.TenantId,
            CommentText = text,
            IsInternal = isInternal
        });
    }

    public async Task<PagedResult<ActivityTimelineDto>> GetTimelineAsync(Guid recordId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        bool canSeeInternal = await _permission.HasPermissionAsync("platform.activity.internal_notes", ct);

        var query = _db.Activities
            .Include(a => a.FieldChanges)
            .Include(a => a.Comments)
            .Where(a => a.RecordId == recordId)
            .Where(a => canSeeInternal || !a.Comments.Any(c => c.IsInternal))
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(a => new ActivityTimelineDto(
            a.Id,
            a.Type,
            a.Message,
            a.CreatedBy,
            a.CreatedAt,
            a.IsSystemGenerated,
            a.FieldChanges.Select(f => new FieldChangeDto(f.FieldName ?? "unknown", f.OldValue, f.NewValue)).ToList(),
            a.Comments.Where(c => canSeeInternal || !c.IsInternal)
                      .Select(c => new CommentDto(c.CommentText, c.IsInternal, c.CreatedBy))
                      .FirstOrDefault()
        )).ToList();

        return new PagedResult<ActivityTimelineDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
