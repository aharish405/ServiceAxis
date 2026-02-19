using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with core metadata (Tables, Fields, Forms) in an idempotent way.
/// Safe to run on every startup; checks for existence before creating.
/// </summary>
public static class MetadataSeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db)
    {
        // 1. Incident Table
        var incident = await EnsureTableAsync(db, "incident", "Incident", "INC");
        await EnsureIncidentFieldsAsync(db, incident.Id);
        await EnsureIncidentFormAsync(db, incident.Id);

        // 2. Service Request Table
        var request = await EnsureTableAsync(db, "service_request", "Service Request", "REQ");
        await EnsureServiceRequestFieldsAsync(db, request.Id);
    }

    private static async Task<SysTable> EnsureTableAsync(ServiceAxisDbContext db, string name, string displayName, string prefix)
    {
        var table = await db.SysTables.FirstOrDefaultAsync(t => t.Name == name);
        if (table == null)
        {
            table = new SysTable 
            { 
                Name = name, 
                DisplayName = displayName, 
                SchemaName = "platform", 
                IsSystemTable = true,
                AutoNumberPrefix = prefix,
                AllowAttachments = true,
                AuditEnabled = true
            };
            db.SysTables.Add(table);
            await db.SaveChangesAsync();
        }
        return table;
    }

    private static async Task EnsureIncidentFieldsAsync(ServiceAxisDbContext db, Guid tableId)
    {
        // If ANY field exists for this table, we assume schema is seeded.
        // More robust logic would check per-field, but this suffices for "initial seed".
        if (await db.SysFields.AnyAsync(f => f.TableId == tableId)) return;

        var fields = new List<SysField>
        {
            new() { TableId = tableId, FieldName = "title", DisplayName = "Title", DataType = FieldDataType.Text, IsRequired = true, DisplayOrder = 10 },
            new() { TableId = tableId, FieldName = "description", DisplayName = "Description", DataType = FieldDataType.LongText, DisplayOrder = 20 },
            new() { TableId = tableId, FieldName = "priority", DisplayName = "Priority", DataType = FieldDataType.Choice, DisplayOrder = 30 },
            new() { TableId = tableId, FieldName = "state", DisplayName = "State", DataType = FieldDataType.Choice, DisplayOrder = 40 }
        };
        db.SysFields.AddRange(fields);
        await db.SaveChangesAsync();

        // Choices
        var priorityField = fields.Single(f => f.FieldName == "priority");
        db.SysChoices.AddRange(
            new SysChoice { FieldId = priorityField.Id, Value = "1", DisplayText = "Critical", Order = 10 },
            new SysChoice { FieldId = priorityField.Id, Value = "2", DisplayText = "High", Order = 20 },
            new SysChoice { FieldId = priorityField.Id, Value = "3", DisplayText = "Medium", Order = 30 },
            new SysChoice { FieldId = priorityField.Id, Value = "4", DisplayText = "Low", Order = 40 }
        );

        var stateField = fields.Single(f => f.FieldName == "state");
        db.SysChoices.AddRange(
            new SysChoice { FieldId = stateField.Id, Value = "new", DisplayText = "New", Order = 10 },
            new SysChoice { FieldId = stateField.Id, Value = "in_progress", DisplayText = "In Progress", Order = 20 },
            new SysChoice { FieldId = stateField.Id, Value = "on_hold", DisplayText = "On Hold", Order = 30 },
            new SysChoice { FieldId = stateField.Id, Value = "resolved", DisplayText = "Resolved", Order = 40 },
            new SysChoice { FieldId = stateField.Id, Value = "closed", DisplayText = "Closed", Order = 50 }
        );
        await db.SaveChangesAsync();
    }

    private static async Task EnsureServiceRequestFieldsAsync(ServiceAxisDbContext db, Guid tableId)
    {
        if (await db.SysFields.AnyAsync(f => f.TableId == tableId)) return;

        db.SysFields.Add(new SysField { TableId = tableId, FieldName = "title", DisplayName = "Title", DataType = FieldDataType.Text, IsRequired = true });
        await db.SaveChangesAsync();
    }

    private static async Task EnsureIncidentFormAsync(ServiceAxisDbContext db, Guid tableId)
    {
        if (await db.FormDefinitions.AnyAsync(f => f.TableId == tableId && f.Name == "incident_default")) return;

        var defaultForm = new FormDefinition 
        { 
            TableId = tableId, 
            Name = "incident_default", 
            FormContext = "default", 
            IsDefault = true, 
            DisplayName = "Default View" 
        };
        db.FormDefinitions.Add(defaultForm);
        await db.SaveChangesAsync();

        var section = new FormSection { FormDefinitionId = defaultForm.Id, Title = "General Information", DisplayOrder = 10, Columns = 2 };
        db.FormSections.Add(section);
        await db.SaveChangesAsync();
        
        var incidentFields = await db.SysFields.Where(f => f.TableId == tableId).ToListAsync();
        
        var title = incidentFields.FirstOrDefault(f => f.FieldName == "title");
        var desc = incidentFields.FirstOrDefault(f => f.FieldName == "description");
        var prio = incidentFields.FirstOrDefault(f => f.FieldName == "priority");
        var state = incidentFields.FirstOrDefault(f => f.FieldName == "state");

        var mappings = new List<FormFieldMapping>();
        if (title != null) mappings.Add(new FormFieldMapping { FormSectionId = section.Id, FieldId = title.Id, DisplayOrder = 10, ColSpan = 2 });
        if (desc != null) mappings.Add(new FormFieldMapping { FormSectionId = section.Id, FieldId = desc.Id, DisplayOrder = 20, ColSpan = 2 });
        if (prio != null) mappings.Add(new FormFieldMapping { FormSectionId = section.Id, FieldId = prio.Id, DisplayOrder = 30 });
        if (state != null) mappings.Add(new FormFieldMapping { FormSectionId = section.Id, FieldId = state.Id, DisplayOrder = 40 });

        if (mappings.Any())
        {
            db.FormFieldMappings.AddRange(mappings);
            await db.SaveChangesAsync();
        }
    }
}
