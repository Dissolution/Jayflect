namespace Jayflect.Extensions;

public static class MethodBaseExtensions
{
    public static Type[] GetParameterTypes(this MethodBase method)
    {
        var parameters = method.GetParameters();
        var count = parameters.Length;
        var types = new Type[count];
        for (var i = count - 1; i >= 0; i--)
        {
            types[i] = parameters[i].ParameterType;
        }
        return types;
    }
}