using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public interface IPropertyGetMethodImplementer
{
    MethodBuilder ImplementGetMethod(FieldBuilder backingField, PropertyBuilder property);
}

internal class DefaultInstancePropertyGetMethodImplementer : Implementer, IPropertyGetMethodImplementer
{
    public DefaultInstancePropertyGetMethodImplementer(TypeBuilder typeBuilder) : base(typeBuilder)
    {
    }

    public MethodBuilder ImplementGetMethod(FieldBuilder backingField, PropertyBuilder property)
    {
        if (property.IsStatic())
            throw new NotImplementedException();

        var getMethod = _typeBuilder.DefineMethod(
                $"get_{property.Name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                GetCallingConventions(property),
                property.PropertyType,
                Type.EmptyTypes)
            .Emit(emitter => emitter.Ldarg_0() // this
                .Ldfld(backingField)
                .Ret());

        property.SetGetMethod(getMethod);
        return getMethod;
    }
}

internal class DefaultStaticPropertyGetMethodImplementer : Implementer, IPropertyGetMethodImplementer
{
    public DefaultStaticPropertyGetMethodImplementer(TypeBuilder typeBuilder) : base(typeBuilder)
    {
    }

    public MethodBuilder ImplementGetMethod(FieldBuilder backingField, PropertyBuilder property)
    {
        if (!property.IsStatic())
            throw new NotImplementedException();

        var getMethod = _typeBuilder.DefineMethod(
                $"get_{property.Name}",
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                GetCallingConventions(property),
                property.PropertyType,
                Type.EmptyTypes)
            .Emit(emitter => emitter.Ldsfld(backingField).Ret());

        property.SetGetMethod(getMethod);
        return getMethod;
    }
}
