using System.Reflection;

namespace Jay.Reflection;

public static class ConstructorInfoExtensions
{
    public static bool HasParameterTypes(this ConstructorInfo constructor,
                                         params Type[] parameterTypes)
    {
        var ctorParams = constructor.GetParameters();
        var len = ctorParams.Length;
        if (parameterTypes.Length != len) return false;
        for (var i = 0; i < len; i++)
        {
            if (ctorParams[i].ParameterType != parameterTypes[i])
                return false;
        }
        return true;
    }
}