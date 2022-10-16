using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public sealed record class PropertyImpl
(
    FieldBuilder BackingField,
    MethodBuilder? GetMethod,
    MethodBuilder? SetMethod,
    PropertyBuilder Property
);