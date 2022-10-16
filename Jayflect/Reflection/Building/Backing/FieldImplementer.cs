using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

internal class FieldImplementer : Implementer, IFieldImplementer
{
    /// <inheritdoc />
    public FieldImplementer(TypeBuilder typeBuilder) : base(typeBuilder)
    {
    }

    /// <inheritdoc />
    public FieldBuilder ImplementField(FieldInfo field)
    {
        var fieldBuilder = _typeBuilder.DefineField(
            field.Name,
            field.FieldType,
            field.Attributes);
        AttributeImplementer.ImplementAttributes(field, fieldBuilder.SetCustomAttribute);
        return fieldBuilder;
    }
}