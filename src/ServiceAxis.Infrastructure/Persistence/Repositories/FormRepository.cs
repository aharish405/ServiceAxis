using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Forms;

namespace ServiceAxis.Infrastructure.Persistence.Repositories;

public class FormRepository : GenericRepository<FormDefinition>, IFormRepository
{
    public FormRepository(ServiceAxisDbContext db) : base(db) { }

    public async Task<FormDefinition?> GetFormWithSectionsAsync(string tableName, string context, CancellationToken ct = default)
    {
        return await Db.FormDefinitions
            .Include(f => f.Table)
            .Include(f => f.Sections)
                .ThenInclude(s => s.FieldMappings)
                    .ThenInclude(m => m.Field)
            .Where(f => f.Table.Name == tableName && f.FormContext == context && f.IsActive)
            .OrderByDescending(f => f.IsDefault)
            .FirstOrDefaultAsync(ct);
    }
}
