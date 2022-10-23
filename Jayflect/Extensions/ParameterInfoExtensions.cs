using System.Diagnostics;
using Jay.Validation;

namespace Jayflect.Extensions;

public static class ParameterInfoExtensions
{
    public static ParameterAccess GetAccess(this ParameterInfo parameter, out Type parameterType)
    {
        parameterType = parameter.ParameterType;
        if (parameterType.IsByRef)
        {
            parameterType = parameterType.GetElementType().ValidateNotNull();
            if (parameter.IsIn)
            {
                return ParameterAccess.In;
            }

            if (parameter.IsOut)
            {
                return ParameterAccess.Out;
            }

            return ParameterAccess.Ref;
        }

        if (parameter.IsIn || parameter.IsOut)
            throw new NotImplementedException();
        
        return ParameterAccess.Default;
    }

    [return: NotNullIfNotNull("parameter")]
    public static Type? NonRefType(this ParameterInfo? parameter)
    {
        if (parameter is null) return null;
        var parameterType = parameter.ParameterType;
        if (parameterType.IsByRef || parameterType.IsByRefLike || parameterType.IsPointer)
        {
            parameterType = parameterType.GetElementType();
            Debug.Assert(parameterType != null);
        }
        return parameterType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsParams(this ParameterInfo parameter) 
        => Attribute.IsDefined(parameter, typeof(ParamArrayAttribute), true);

    /// <summary>
    /// Is this <see cref="ParameterInfo"/> for an <see cref="object"/> <see cref="Array"/>?
    /// </summary>
    public static bool IsObjectArray(this ParameterInfo parameter)
    {
        return !parameter.IsIn &&
               !parameter.IsOut &&
               parameter.ParameterType == typeof(object[]);
    }
}