using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public interface IPropertySetMethodImplementer
{
    MethodBuilder? ImplementSetMethod(FieldBuilder backingField, PropertyBuilder property);
}

internal class DefaultInstancePropertySetMethodImplementer : Implementer, IPropertySetMethodImplementer
{
    /// <inheritdoc />
    public DefaultInstancePropertySetMethodImplementer(TypeBuilder typeBuilder) 
        : base(typeBuilder)
    {
    }

    /// <inheritdoc />
    public MethodBuilder? ImplementSetMethod(FieldBuilder backingField, PropertyBuilder property)
    {
        if (property.IsStatic())
            throw new NotImplementedException();

        var setMethod = _typeBuilder.DefineMethod(
                $"set_{property.Name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                GetCallingConventions(property),
                typeof(void),
                new Type[1] { property.PropertyType })
            .Emit(emitter => emitter.Ldarg_0() // this
                .Ldarg_1() // value
                .Stfld(backingField) // this._field = value
                .Ret());

        property.SetSetMethod(setMethod);
        return setMethod;
    }
}

internal class NotifyPropertySetMethodImplementer : Implementer, IPropertySetMethodImplementer
{
    private readonly MethodInfo? _onPropertyChanging = null;
    private readonly MethodInfo? _onPropertyChanged = null;

    /// <inheritdoc />
    public NotifyPropertySetMethodImplementer(TypeBuilder typeBuilder,
        MethodInfo? onPropertyChanging,
        MethodInfo? onPropertyChanged)
        : base(typeBuilder)
    {
        if (onPropertyChanging is not null)
        {
            if (onPropertyChanging.ReturnType != typeof(void) ||
                onPropertyChanging.GetParameters().Length != 2 ||
                onPropertyChanging.GetParameters()[0].ParameterType != typeof(object) ||
                onPropertyChanging.GetParameters()[1].ParameterType != typeof(PropertyChangingEventArgs))
            {
                throw new ArgumentException("", nameof(onPropertyChanging));
            }
            _onPropertyChanging = onPropertyChanging;
        }

        if (onPropertyChanged is not null)
        {
            if (onPropertyChanged.ReturnType != typeof(void) ||
                onPropertyChanged.GetParameters().Length != 2 ||
                onPropertyChanged.GetParameters()[0].ParameterType != typeof(object) ||
                onPropertyChanged.GetParameters()[1].ParameterType != typeof(PropertyChangedEventArgs))
            {
                throw new ArgumentException("", nameof(onPropertyChanged));
            }
            _onPropertyChanged = onPropertyChanged;
        }
    }

    /// <inheritdoc />
    public MethodBuilder? ImplementSetMethod(FieldBuilder backingField, PropertyBuilder property)
    {
        if (property.IsStatic())
            throw new NotImplementedException();

        var setMethod = _typeBuilder.DefineMethod(
            $"set_{property.Name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
            GetCallingConventions(property),
            typeof(void),
            new Type[1] { property.PropertyType });
        /* What we're (approximately) emitting:
         * set_Property(T value)
         * {
         *   if (value == _field) return;
         *   OnPropertyChanging?(this, new PropertyChangingEventArgs(nameof(Property)));
         *   _field = value;
         *   OnPropertyChanged?(this, new PropertyChangedEventArgs(nameof(Property)));
         * }
         */
        var emitter = setMethod.GetILEmitter();
        emitter.Ldarg_0()
            .Ldfld(backingField)
            .Ldarg_1()
            .Beq(out var lblReturn);
        if (_onPropertyChanging is not null)
        {
            var ctor = Reflect.On<PropertyChangingEventArgs>().GetConstructor(typeof(string));
            Debugger.Break();
            emitter.Ldarg_0()
                .Ldarg_0()
                .Cast(_typeBuilder, typeof(object))
                .Ldstr(property.Name)
                .Newobj(ctor)
                .Call(_onPropertyChanging);
        }

        emitter.Ldarg_0()
            .Ldarg_1()
            .Stfld(backingField);
        
        if (_onPropertyChanged is not null)
        {
            var ctor = Reflect.On<PropertyChangedEventArgs>().GetConstructor(typeof(string));
            Debugger.Break();
            emitter.Ldarg_0()
                .Ldarg_0()
                .Cast(_typeBuilder, typeof(object))
                .Ldstr(property.Name)
                .Newobj(ctor)
                .Call(_onPropertyChanged);
        }  

        property.SetSetMethod(setMethod);
        return setMethod;
    }
}

internal class DefaultStaticPropertySetMethodImplementer : Implementer, IPropertySetMethodImplementer
{
    /// <inheritdoc />
    public DefaultStaticPropertySetMethodImplementer(TypeBuilder typeBuilder) 
        : base(typeBuilder)
    {
    }

    /// <inheritdoc />
    public MethodBuilder? ImplementSetMethod(FieldBuilder backingField, PropertyBuilder property)
    {
        if (!property.IsStatic())
            throw new NotImplementedException();

        var setMethod = _typeBuilder.DefineMethod(
                $"set_{property.Name}",
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                GetCallingConventions(property),
                typeof(void),
                new Type[1] { property.PropertyType })
            .Emit(emitter => emitter.Ldarg_0() // value
                .Stsfld(backingField) // _field = value
                .Ret());

        property.SetSetMethod(setMethod);
        return setMethod;
    }
}