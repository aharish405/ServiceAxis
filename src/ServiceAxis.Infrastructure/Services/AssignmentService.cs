using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly ServiceAxisDbContext _db;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(ServiceAxisDbContext db, ILogger<AssignmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AssignAsync(
        Guid recordId,
        string? userId,
        Guid? groupId,
        CancellationToken ct = default)
    {
        var record = await _db.PlatformRecords.FindAsync(new object[] { recordId }, ct);
        if (record == null) throw new NotFoundException("Record", recordId);

        record.AssignedToUserId = userId;
        record.AssignmentGroupId = groupId;
        record.UpdatedAt = DateTime.UtcNow;
        
        // Update member metrics if assigned to a user in a group
        if (groupId.HasValue && !string.IsNullOrEmpty(userId))
        {
             var member = await _db.Set<GroupMember>()
                 .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, ct);
             
             if (member != null)
             {
                 member.ActiveItemCount++;
             }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Assigned Record {RecordId} to User={User} Group={Group}", recordId, userId, groupId);
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
            
        if (queue == null || queue.Group == null) return null;

        var availableMembers = queue.Group.Members
            .Where(m => m.IsAvailable)
            .ToList();

        if (!availableMembers.Any()) return null;

        GroupMember? selectedMember = null;
        var strategy = queue.Strategy;

        // Simplified strategy logic
        if (strategy == AssignmentStrategy.RoundRobin || strategy == AssignmentStrategy.LeastLoaded)
        {
             // Pick member with least active items
             selectedMember = availableMembers.OrderBy(m => m.ActiveItemCount).FirstOrDefault();
        }
        else if (strategy == AssignmentStrategy.Manual)
        {
            return null;
        }

        if (selectedMember != null)
        {
            await AssignAsync(recordId, selectedMember.UserId, queue.GroupId, ct);
            return selectedMember.UserId;
        }

        return null;
    }

    public async Task<Guid?> ResolveQueueAsync(
        string tableName,
        int priority,
        CancellationToken ct = default)
    {
        // Simple router: match table, pick highest priority (lowest number)
        var queue = await _db.Queues
            .Where(q => q.TableName == tableName && q.IsActive)
            .OrderBy(q => q.Priority)
            .FirstOrDefaultAsync(ct);
            
        return queue?.Id;
    }
}
