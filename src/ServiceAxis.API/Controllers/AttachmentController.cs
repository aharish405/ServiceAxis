using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Application.Features.Activity.Commands;
using ServiceAxis.Domain.Entities.Activity;

namespace ServiceAxis.API.Controllers;

[ApiController]
[Route("api/v1/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageProvider _storage;
    private readonly IUnitOfWork _uow;

    public AttachmentController(ISender sender, IFileStorageProvider storage, IUnitOfWork uow)
    {
        _sender = sender;
        _storage = storage;
        _uow = uow;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var attachment = await _uow.Repository<Attachment>().GetByIdAsync(id);
        if (attachment == null) return NotFound();

        var stream = await _storage.GetFileAsync(attachment.StoragePath);
        return File(stream, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteAttachmentCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }
}
