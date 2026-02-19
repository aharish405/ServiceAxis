using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IFieldTypeService
{
    object? ConvertToTyped(string? value, FieldDataType dataType);
    string? ConvertToString(object? value, FieldDataType dataType);
    bool Validate(string? value, FieldDataType dataType, out string? errorMessage);
}
