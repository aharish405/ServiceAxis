using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Application.Common.Models;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly ServiceAxisDbContext     _db;
    private readonly IActivityService        _activity;
    private readonly IPlatformEventPublisher _events;
    private readonly ICurrentUserService     _currentUser;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(
        ServiceAxisDbContext db,
        IActivityService activity,
        IPlatformEventPublisher events,
        ICurrentUserService currentUser,
        ILogger<AssignmentService> logger)
    {
        _db          = db;
        _activity    = activity;
        _events      = events;
        _currentUser = currentUser;
        _logger      = logger;
    }

    public async Task AssignAsync(
        Guid recordId,
        string? userId,
        Guid? groupId,
        CancellationToken ct = default)
    {
        var record = await _db.PlatformRecords.FindAsync(new object[] { recordId }, ct)
            ?? throw new NotFoundException("Record", recordId);

        var oldUser  = record.AssignedToUserId;
        var oldGroup = record.AssignmentGroupId;

        record.AssignedToUserId    = userId;
        record.AssignmentGroupId   = groupId;
        record.UpdatedAt           = DateTime.UtcNow;

        // Update group member metrics
        if (groupId.HasValue && !string.IsNullOrEmpty(userId))
        {
            var member = await _db.Set<GroupMember>()
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, ct);
            if (member != null) member.ActiveItemCount++;
        }

        // ── Write audit row ───────────────────────────────────────────────────
        _db.RecordAssignments.Add(new RecordAssignment
        {
            RecordId       = recordId,
            TableId        = record.TableId,
            AssignedUserId = userId,
            AssignedGroupId = groupId,
            AssignedAt     = DateTime.UtcNow,
            AssignedBy     = _currentUser.UserId
        });

        // ── Activity entry ────────────────────────────────────────────────────
        var changes = new List<(string FieldName, Guid? FieldId, string? OldValue, string? NewValue)>();
        if (oldUser != userId)
            changes.Add(("assigned_to", null, oldUser, userId));
        if (oldGroup != groupId)
            changes.Add(("assignment_group", null, oldGroup?.ToString(), groupId?.ToString()));

        if (changes.Any())
        {
            await _activity.LogActivityAsync(
                record.TableId,
                recordId,
                ActivityType.AssignmentChanged,
                BuildAssignmentMessage(userId, groupId),
                isSystem: false,
                changes: changes,
                ct: ct);

            // Publish Event
            var tableName = await _db.SysTables.Where(t => t.Id == record.TableId).Select(t => t.Name).FirstOrDefaultAsync(ct) ?? "";
            await _events.PublishAsync(new AssignmentChangedEvent {
                RecordId = recordId,
                TableId = record.TableId,
                TableName = tableName,
                TenantId = record.TenantId,
                OldUserId = oldUser,
                NewUserId = userId,
                OldGroupId = oldGroup,
                NewGroupId = groupId
            });
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Assigned Record {RecordId} → User={User} Group={Group}", recordId, userId, groupId);
    }

    public async Task<string?> AutoAssignAsync(
        Guid recordId,
        string tableName,
        int priority,
        CancellationToken ct = default)
    {
        var queueId = await ResolveQueueAsync(tableName, priority, ct);
        if (queueId == null)
        {
            _logger.LogWarning("No queue found for auto-assignment (Table={Table})", tableName);
            return null;
        }

        var queue = await _db.Queues
            .Include(q => q.Group)
            .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, ct);

        if (queue?.Group == null) return null;

        var availableMembers = queue.Group.Members.Where(m => m.IsAvailable).ToList();
        if (!availableMembers.Any()) return null;

        // Always pick least-loaded member regardless of strategy for now (RoundRobin ≈ LeastLoaded at low volume)
        var selected = availableMembers.OrderBy(m => m.ActiveItemCount).First();
        await AssignAsync(recordId, selected.UserId, queue.GroupId, ct);
        return selected.UserId;
    }

    public async Task<Guid?> ResolveQueueAsync(
        string tableName,
        int priority,
        CancellationToken ct = default)
    {
        var queue = await _db.Queues
            .Where(q => q.TableName == tableName && q.IsActive)
            .OrderBy(q => q.Priority)
            .FirstOrDefaultAsync(ct);
        return queue?.Id;
    }

    private static string BuildAssignmentMessage(string? userId, Guid? groupId)
    {
        if (!string.IsNullOrEmpty(userId) && groupId.HasValue)
            return $"Assigned to user '{userId}' in group '{groupId}'.";
        if (!string.IsNullOrEmpty(userId))
            return $"Assigned to user '{userId}'.";
        if (groupId.HasValue)
            return $"Routed to group '{groupId}'.";
        return "Assignment cleared.";
    }
}
