using ServiceAxis.Domain.Entities.Forms;

namespace ServiceAxis.Application.Contracts.Persistence;

public interface IFormRepository : IGenericRepository<FormDefinition>
{
    Task<FormDefinition?> GetFormWithSectionsAsync(string tableName, string context, CancellationToken ct = default);
}
