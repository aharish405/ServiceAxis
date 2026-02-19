using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

public class SlaService : ISlaService
{
    private readonly ServiceAxisDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SlaService> _logger;

    public SlaService(
        ServiceAxisDbContext db,
        INotificationService notificationService,
        ILogger<SlaService> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SlaInstance?> StartSlaAsync(
        Guid recordId,
        string tableName,
        int priority,
        CancellationToken ct = default)
    {
        // Find Active Definition for this table
        var definition = await _db.SlaDefinitions
            .Include(d => d.Policies)
            .Where(d => d.TableName == tableName && d.IsActive)
            .FirstOrDefaultAsync(ct);

        if (definition == null) return null;

        // Find Policy matching Priority
        // Assuming priority integer maps directly to SlaPriority enum (1=Critical, etc.)
        // This mapping logic should be robust but strict cast for MVP.
        if (!Enum.IsDefined(typeof(SlaPriority), priority))
        {
            _logger.LogWarning("Invalid priority {Priority} for SLA calculation.", priority);
            return null;
        }
        var slaPriority = (SlaPriority)priority;

        var policy = definition.Policies.FirstOrDefault(p => p.Priority == slaPriority);
        if (policy == null) return null;

        // Calculate Deadlines
        // TODO: Implement Schedule Engine (Business Hours). 
        // Current implementation assumes 24x7 coverage.
        var now = DateTime.UtcNow;
        var responseDue = now.AddMinutes(policy.ResponseTimeMinutes);
        var resolutionDue = now.AddMinutes(policy.ResolutionTimeMinutes);

        var instance = new SlaInstance
        {
            RecordId = recordId,
            TableName = tableName,
            SlaDefinitionId = definition.Id,
            SlaPolicyId = policy.Id,
            Status = SlaStatus.Active,
            StartedAt = now,
            ResponseDueAt = responseDue,
            ResolutionDueAt = resolutionDue,
            TenantId = definition.TenantId
        };

        _db.Set<SlaInstance>().Add(instance);
        await _db.SaveChangesAsync(ct);
        
        _logger.LogInformation("Started SLA for Record {RecordId}. Due: {ResolutionDue}", recordId, resolutionDue);
        return instance;
    }

    public async Task PauseSlaAsync(Guid recordId, CancellationToken ct = default)
    {
        var instance = await GetActiveSlaAsync(recordId, ct);
        if (instance == null) return;

        instance.Status = SlaStatus.Paused;
        instance.PausedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task ResumeSlaAsync(Guid recordId, CancellationToken ct = default)
    {
        var instance = await _db.Set<SlaInstance>()
            .FirstOrDefaultAsync(s => s.RecordId == recordId && s.Status == SlaStatus.Paused, ct);

        if (instance == null || !instance.PausedAt.HasValue) return;

        var now = DateTime.UtcNow;
        var pausedDuration = now - instance.PausedAt.Value;
        
        // Add duration to total paused time
        instance.PausedMinutes += (int)pausedDuration.TotalMinutes; // truncation is acceptable for MVP
        
        // Push deadlines forward by paused duration
        instance.ResponseDueAt = instance.ResponseDueAt.Add(pausedDuration);
        instance.ResolutionDueAt = instance.ResolutionDueAt.Add(pausedDuration);
        
        instance.Status = SlaStatus.Active;
        instance.PausedAt = null;

        await _db.SaveChangesAsync(ct);
    }

    public async Task CompleteSlaAsync(Guid recordId, CancellationToken ct = default)
    {
        var instance = await GetActiveSlaAsync(recordId, ct);
        if (instance == null) return;

        instance.Status = SlaStatus.Met; // Met successful resolution
        instance.ResolvedAt = DateTime.UtcNow;

        // Mark breached if passed deadline? 
        // Usually done by EvaluateAllActiveAsync or check right here.
        if (instance.ResolvedAt > instance.ResolutionDueAt)
        {
            instance.ResolutionBreachFired = true; // Historical record that it breached
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task EvaluateAllActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var activeInstances = await _db.Set<SlaInstance>()
            .Include(i => i.SlaPolicy)
            .Where(i => i.Status == SlaStatus.Active)
            .ToListAsync(ct);

        foreach (var instance in activeInstances)
        {
            // Check Response Breach
            if (!instance.RespondedAt.HasValue && !instance.ResponseBreachFired && now > instance.ResponseDueAt)
            {
                instance.ResponseBreachFired = true;
                // instance.Status = SlaStatus.Breached; // Optional: change main status
                await NotifyBreachAsync(instance, "Response", ct);
            }

            // Check Resolution Breach
            if (!instance.ResolutionBreachFired && now > instance.ResolutionDueAt)
            {
                instance.ResolutionBreachFired = true;
                // instance.Status = SlaStatus.Breached;
                await NotifyBreachAsync(instance, "Resolution", ct);
            }
            
            // Check Warnings (e.g. at 75%) can be added here
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<SlaInstance?> GetActiveSlaAsync(Guid recordId, CancellationToken ct)
    {
        return await _db.Set<SlaInstance>()
            .FirstOrDefaultAsync(s => s.RecordId == recordId && s.Status == SlaStatus.Active, ct);
    }

    private async Task NotifyBreachAsync(SlaInstance instance, string type, CancellationToken ct)
    {
        if (instance.SlaPolicy.NotifyOnBreach)
        {
             // Send notification to assignee/group?
             // Need to lookup record to find assignee.
             // For now, log warning.
             _logger.LogWarning("SLA {Type} Breach for Record {RecordId}. Policy: {PolicyId}", type, instance.RecordId, instance.SlaPolicyId);
             
             // TODO: Call _notificationService.SendAsync("SLA_BREACH", ...)
        }
        await Task.CompletedTask;
    }
}
