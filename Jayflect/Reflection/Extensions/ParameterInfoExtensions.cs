using System.Diagnostics;
using System.Reflection;

namespace Jay.Reflection;

public static class ParameterInfoExtensions
{
   

    public static ParameterAccess GetAccess(this ParameterInfo parameter, out Type parameterType)
    {
        parameterType = parameter.ParameterType;
        if (parameterType.IsByRef)
        {
            parameterType = parameterType.GetElementType()!;
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

}