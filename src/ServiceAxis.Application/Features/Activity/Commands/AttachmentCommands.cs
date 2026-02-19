using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Activity;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Features.Activity.Commands;

public record AddAttachmentCommand(
    string TableName,
    Guid RecordId,
    string FileName,
    string ContentType,
    long FileSize,
    Stream Content) : IRequest<Guid>;

public class AddAttachmentHandler : IRequestHandler<AddAttachmentCommand, Guid>
{
    private readonly IFileStorageProvider _storage;
    private readonly IActivityService _activity;
    private readonly IMetadataCache _cache;
    private readonly IUnitOfWork _uow;

    public AddAttachmentHandler(
        IFileStorageProvider storage,
        IActivityService activity,
        IMetadataCache cache,
        IUnitOfWork uow)
    {
        _storage = storage;
        _activity = activity;
        _cache = cache;
        _uow = uow;
    }

    public async Task<Guid> Handle(AddAttachmentCommand request, CancellationToken ct)
    {
        var table = await _cache.GetTableAsync(request.TableName, ct);
        var tableId = table?.Id ?? Guid.Empty;

        var storagePath = await _storage.SaveFileAsync(request.Content, request.FileName, request.TableName, ct);

        var attachment = new Attachment
        {
            RecordId = request.RecordId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow
        };

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.Repository<Attachment>().AddAsync(attachment, ct);

            await _activity.LogActivityAsync(
                tableId,
                request.RecordId,
                ActivityType.AttachmentAdded,
                $"Attachment '{request.FileName}' added.",
                isSystem: true,
                ct: ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            // Cleanup file if DB save fails
            await _storage.DeleteFileAsync(storagePath, ct);
            throw;
        }

        return attachment.Id;
    }
}

public record DeleteAttachmentCommand(Guid AttachmentId) : IRequest<bool>;

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachmentCommand, bool>
{
    private readonly IFileStorageProvider _storage;
    private readonly IUnitOfWork _uow;

    public DeleteAttachmentHandler(IFileStorageProvider storage, IUnitOfWork uow)
    {
        _storage = storage;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        var attachment = await _uow.Repository<Attachment>().GetByIdAsync(request.AttachmentId, ct);
        if (attachment == null) return false;

        await _storage.DeleteFileAsync(attachment.StoragePath, ct);
        _uow.Repository<Attachment>().Delete(attachment);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}
