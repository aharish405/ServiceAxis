namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IFileStorageProvider
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string context, CancellationToken ct = default);
    Task<Stream> GetFileAsync(string storagePath, CancellationToken ct = default);
    Task DeleteFileAsync(string storagePath, CancellationToken ct = default);
}
