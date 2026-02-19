using Microsoft.Extensions.Configuration;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.Infrastructure.Services;

public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _storageRoot;

    public LocalFileStorageProvider(IConfiguration configuration)
    {
        _storageRoot = configuration["Storage:LocalRoot"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string context, CancellationToken ct = default)
    {
        var relativePath = Path.Combine(context, Guid.NewGuid().ToString("N") + Path.GetExtension(fileName));
        var fullPath = Path.Combine(_storageRoot, relativePath);
        
        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await fileStream.CopyToAsync(outputStream, ct);

        return relativePath;
    }

    public Task<Stream> GetFileAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storageRoot, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Attachment file not found on disk.", storagePath);
        }

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true));
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storageRoot, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
