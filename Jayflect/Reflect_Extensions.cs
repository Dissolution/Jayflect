using Jay;
using Jay.Validation;
using Jayflect.Building;
using Jayflect.Building.Emission;
using Jayflect.Caching;
using Jayflect.Extensions;

namespace Jayflect;

public delegate void SetValue<T, in TValue>(ref T instance, TValue value);

public delegate TValue GetValue<T, out TValue>(ref T instance);


public static partial class Reflect
{
    private static IFluentILEmitter EmitLoadParameter(this IFluentILEmitter emitter,
        ParameterInfo parameter, ParameterSignature destParameter)
    {
        ParameterLoader.TryLoadParameter(emitter, parameter, destParameter)
            .ThrowIfFailed();
        return emitter;
    }
    
    private static IFluentILEmitter EmitLoadInstance(this IFluentILEmitter emitter,
        ParameterInfo? instanceParameter, MemberInfo member)
    {
        if (member.IsStatic())
        {
            // Do nothing
            return emitter;
        }
        else
        {
            if (instanceParameter is null)
                throw new AdapterException($"Required instance parameter is missing");
            var ownerType = member.OwnerType();
            if (ownerType.IsValueType)
            {
                // We need it as a ref
                ownerType = ownerType.MakeByRefType();
            }
            ParameterLoader.TryLoadParameter(emitter, instanceParameter, ownerType)
                .ThrowIfFailed();
        }
    }
    
    private static SetValue<T, TValue> CreateSetValue<T, TValue>(FieldInfo field)
    {
        return RuntimeBuilder.CreateDelegate<SetValue<T, TValue>>($"set_{field.Name}",
            builder => builder.Emitter
                .EmitLoadInstance(builder.Parameters[0], field)
                .EmitLoadParameter(builder.Parameters[1], field.FieldType)
                .Stfld(field)
                .Ret());
    }
    
    private static GetValue<T, TValue> CreateGetValue<T, TValue>(FieldInfo field)
    {
        return RuntimeBuilder.CreateDelegate<GetValue<T, TValue>>($"get_{field.Name}",
            builder => builder.Emitter
                .EmitLoadInstance(builder.Parameters[0], field)
                .Ldfld(field)
                .EmitCast(field.FieldType, builder.ReturnType)
                .Ret());
    }

    public static void SetValue<T, TValue>(this FieldInfo fieldInfo,
        ref T instance, TValue value)
    {
        var setValue = MemberDelegateCache.GetOrAdd(fieldInfo, CreateSetValue<T, TValue>);
        setValue(ref instance, value);
    }
    public static TValue GetValue<T, TValue>(this FieldInfo fieldInfo,
        ref T instance)
    {
        var getValue = MemberDelegateCache.GetOrAdd(fieldInfo, CreateGetValue<T, TValue>);
        return getValue(ref instance);
    }

    private static SetValue<T, TValue> CreateSetValue<T, TValue>(PropertyInfo property)
    {
        // Find setter
        var setter = property.GetSetter();
        if (setter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is not null)
                return CreateSetValue<T, TValue>(backingField);
            throw new AdapterException($"Cannot find a Setter");
        }

        return RuntimeMethodAdapter.Adapt<SetValue<T, TValue>>(setter);
    }
    
    private static GetValue<T, TValue> CreateGetValue<T, TValue>(PropertyInfo property)
    {
        // Find getter
        var getter = property.GetGetter();
        if (getter is null)
        {
            var backingField = property.GetBackingField();
            if (backingField is not null)
                return CreateGetValue<T, TValue>(backingField);
            throw new AdapterException($"Cannot find a Getter");
        }

        return RuntimeMethodAdapter.Adapt<GetValue<T, TValue>>(getter);
    }
    
    public static void SetValue<T, TValue>(this PropertyInfo propertyInfo,
        ref T instance, TValue value)
    {
        var setValue = MemberDelegateCache.GetOrAdd(propertyInfo, CreateSetValue<T, TValue>);
        setValue(ref instance, value);
    }
    public static TValue GetValue<T, TValue>(this PropertyInfo propertyInfo,
        ref T instance)
    {
        var getValue = MemberDelegateCache.GetOrAdd(propertyInfo, CreateGetValue<T, TValue>);
        return getValue(ref instance);
    }
}