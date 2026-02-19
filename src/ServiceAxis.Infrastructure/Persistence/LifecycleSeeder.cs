using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Persistence;

public static class LifecycleSeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db)
    {
        // 1. Seed Assignment Groups
        await SeedGroupsAsync(db);

        // 2. Seed Incident Lifecycle
        await SeedIncidentLifecycleAsync(db);

        // 3. Seed Queues (optional)
        await SeedQueuesAsync(db);
    }

    private static async Task SeedGroupsAsync(ServiceAxisDbContext db)
    {
        if (await db.AssignmentGroups.AnyAsync(g => g.Name == "Service Desk"))
            return;

        var group = new AssignmentGroup
        {
            Name = "Service Desk",
            Description = "Tier 1 Support - General Inquiries & Incidents",
            Email = "helpdesk@serviceaxis.io",
            DefaultStrategy = AssignmentStrategy.RoundRobin,
            IsActive = true
        };

        db.AssignmentGroups.Add(group);
        await db.SaveChangesAsync();
    }

    private static async Task SeedQueuesAsync(ServiceAxisDbContext db)
    {
        if (await db.Queues.AnyAsync()) return;

        var serviceDesk = await db.AssignmentGroups.FirstOrDefaultAsync(g => g.Name == "Service Desk");
        if (serviceDesk == null) return;

        var queue = new Queue
        {
            Name = "Incident Triage",
            GroupId = serviceDesk.Id,
            TableName = "incident",
            Priority = 10,
            Strategy = AssignmentStrategy.RoundRobin,
            IsActive = true
        };

        db.Queues.Add(queue);
        await db.SaveChangesAsync();
    }

    private static async Task SeedIncidentLifecycleAsync(ServiceAxisDbContext db)
    {
        var incidentTable = await db.SysTables.FirstOrDefaultAsync(t => t.Name == "incident");
        if (incidentTable == null) return; // Metadata not seeded yet

        if (await db.RecordStateDefinitions.AnyAsync(s => s.TableId == incidentTable.Id))
            return; // Already seeded

        // ── Define States ──
        var newState = new RecordStateDefinition
        {
            TableId = incidentTable.Id,
            StateName = "new",
            DisplayName = "New",
            IsInitialState = true,
            Order = 10,
            Color = "gray"
        };

        var inProgress = new RecordStateDefinition
        {
            TableId = incidentTable.Id,
            StateName = "in_progress",
            DisplayName = "In Progress",
            Order = 20,
            Color = "blue"
        };

        var pending = new RecordStateDefinition
        {
            TableId = incidentTable.Id,
            StateName = "pending",
            DisplayName = "Pending",
            Order = 30,
            Color = "orange"
        };

        var resolved = new RecordStateDefinition
        {
            TableId = incidentTable.Id,
            StateName = "resolved",
            DisplayName = "Resolved",
            Order = 40,
            Color = "green"
        };

        var closed = new RecordStateDefinition
        {
            TableId = incidentTable.Id,
            StateName = "closed",
            DisplayName = "Closed",
            IsFinalState = true,
            Order = 50,
            Color = "slate"
        };

        db.RecordStateDefinitions.AddRange(newState, inProgress, pending, resolved, closed);
        await db.SaveChangesAsync();

        // ── Define Transitions ──
        var transitions = new List<StateTransition>
        {
            // New -> In Progress
            new() { TableId = incidentTable.Id, FromStateId = newState.Id, ToStateId = inProgress.Id, Label = "Start Work" },
            
            // In Progress -> Pending
            new() { TableId = incidentTable.Id, FromStateId = inProgress.Id, ToStateId = pending.Id, Label = "Hold" },
            
            // Pending -> In Progress
            new() { TableId = incidentTable.Id, FromStateId = pending.Id, ToStateId = inProgress.Id, Label = "Resume" },
            
            // In Progress -> Resolved
            new() { TableId = incidentTable.Id, FromStateId = inProgress.Id, ToStateId = resolved.Id, Label = "Resolve" },
            
            // Resolved -> Closed
            new() { TableId = incidentTable.Id, FromStateId = resolved.Id, ToStateId = closed.Id, Label = "Close" },
            
            // Resolved -> In Progress (Re-open) - Optional but useful
            new() { TableId = incidentTable.Id, FromStateId = resolved.Id, ToStateId = inProgress.Id, Label = "Re-open" }
        };

        db.StateTransitions.AddRange(transitions);
        await db.SaveChangesAsync();
    }
}
