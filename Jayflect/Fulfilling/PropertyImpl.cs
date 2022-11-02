namespace Jayflect.Fulfilling;

public sealed record class PropertyImpl
(
    FieldBuilder BackingField,
    MethodBuilder? GetMethod,
    MethodBuilder? SetMethod,
    PropertyBuilder Property
);