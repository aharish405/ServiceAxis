using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Forms.Commands;

public record SaveFormLayoutCommand(
    Guid TableId,
    string Context,
    List<SaveFormSectionDto> Sections) : IRequest<Unit>;

public record SaveFormSectionDto(
    string Title,
    int DisplayOrder,
    int Columns,
    bool IsCollapsed,
    List<SaveFormFieldMappingDto> Fields);

public record SaveFormFieldMappingDto(
    Guid FieldId,
    int DisplayOrder,
    int ColSpan,
    bool IsReadOnlyOverride);

public class SaveFormLayoutHandler : IRequestHandler<SaveFormLayoutCommand, Unit>
{
    private readonly IFormRepository _forms;
    private readonly ISysTableRepository _tables;
    private readonly IUnitOfWork _uow;

    public SaveFormLayoutHandler(IFormRepository forms, ISysTableRepository tables, IUnitOfWork uow)
    {
        _forms = forms;
        _tables = tables;
        _uow = uow;
    }

    public async Task<Unit> Handle(SaveFormLayoutCommand cmd, CancellationToken ct)
    {
        var tableExists = await _tables.GetByIdAsync(cmd.TableId, ct) != null;
        if (!tableExists) throw new NotFoundException($"Table '{cmd.TableId}' not found.");

        var existingForms = await _forms.FindAsync(f => f.TableId == cmd.TableId && f.FormContext == cmd.Context, ct);
        var existingForm = existingForms.FirstOrDefault();

        if (existingForm != null)
        {
            _forms.Delete(existingForm);
            await _uow.SaveChangesAsync(ct);
        }

        var form = new FormDefinition
        {
            TableId = cmd.TableId,
            FormContext = cmd.Context,
            Name = $"{cmd.TableId}_{cmd.Context}_form",
            DisplayName = "Auto Generated Form",
            IsDefault = true,
            Sections = cmd.Sections.Select(s => new FormSection
            {
                Title = s.Title,
                DisplayOrder = s.DisplayOrder,
                Columns = s.Columns,
                IsCollapsed = s.IsCollapsed,
                FieldMappings = s.Fields.Select(f => new FormFieldMapping
                {
                    FieldId = f.FieldId,
                    DisplayOrder = f.DisplayOrder,
                    ColSpan = f.ColSpan,
                    IsReadOnlyOverride = f.IsReadOnlyOverride
                }).ToList()
            }).ToList()
        };

        await _forms.AddAsync(form, ct);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
