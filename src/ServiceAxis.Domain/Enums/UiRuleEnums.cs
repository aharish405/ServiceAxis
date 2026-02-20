using System.Text.Json.Serialization;

namespace ServiceAxis.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UiPolicyOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    IsEmpty,
    IsNotEmpty
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UiPolicyActionType
{
    Show,
    Hide,
    MakeMandatory,
    MakeOptional,
    MakeReadOnly,
    MakeEditable
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FieldRuleActionType
{
    SetValue,
    Calculate,
    CopyField
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClientScriptEventType
{
    OnLoad,
    OnChange,
    OnSubmit
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormContextType
{
    All,
    Create,
    Edit,
    View
}
