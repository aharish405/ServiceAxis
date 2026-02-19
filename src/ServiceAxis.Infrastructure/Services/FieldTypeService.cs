using System.Globalization;
using System.Text.Json;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Services;

public class FieldTypeService : IFieldTypeService
{
    public object? ConvertToTyped(string? value, FieldDataType dataType)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            return dataType switch
            {
                FieldDataType.Number => decimal.Parse(value, CultureInfo.InvariantCulture),
                FieldDataType.Boolean => bool.Parse(value),
                FieldDataType.Date => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                FieldDataType.DateTime => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                FieldDataType.Lookup => Guid.Parse(value),
                FieldDataType.MultiChoice => JsonSerializer.Deserialize<string[]>(value),
                _ => value // Text, LongText, Choice, Json, AutoNumber represent as string
            };
        }
        catch
        {
            return null;
        }
    }

    public string? ConvertToString(object? value, FieldDataType dataType)
    {
        if (value == null) return null;

        return dataType switch
        {
            FieldDataType.Number when value is decimal d => d.ToString(CultureInfo.InvariantCulture),
            FieldDataType.Boolean when value is bool b => b.ToString().ToLowerInvariant(),
            FieldDataType.Date when value is DateTime d => d.ToString("yyyy-MM-dd"),
            FieldDataType.DateTime when value is DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            FieldDataType.MultiChoice when value is IEnumerable<string> e => JsonSerializer.Serialize(e),
            _ => value.ToString()
        };
    }

    public bool Validate(string? value, FieldDataType dataType, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value)) return true;

        switch (dataType)
        {
            case FieldDataType.Number:
                if (!decimal.TryParse(value, CultureInfo.InvariantCulture, out _))
                {
                    errorMessage = "Invalid number format.";
                    return false;
                }
                break;
            case FieldDataType.Boolean:
                if (!bool.TryParse(value, out _))
                {
                    errorMessage = "Invalid boolean value (expected true/false).";
                    return false;
                }
                break;
            case FieldDataType.Date:
                if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    errorMessage = "Invalid date format (expected yyyy-MM-dd).";
                    return false;
                }
                break;
            case FieldDataType.DateTime:
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    errorMessage = "Invalid date-time format.";
                    return false;
                }
                break;
            case FieldDataType.Lookup:
                if (!Guid.TryParse(value, out _))
                {
                    errorMessage = "Invalid lookup ID format.";
                    return false;
                }
                break;
            case FieldDataType.MultiChoice:
                try { JsonSerializer.Deserialize<string[]>(value); }
                catch { errorMessage = "Invalid multi-choice JSON array."; return false; }
                break;
        }

        return true;
    }
}
