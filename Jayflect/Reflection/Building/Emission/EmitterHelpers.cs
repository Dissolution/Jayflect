using System.Reflection;

namespace Jay.Reflection.Building.Emission;

internal static class EmitterHelpers
{
    public static bool IsObjectArray(this ParameterInfo parameter)
    {
        return !parameter.IsIn &&
               !parameter.IsOut &&
               parameter.ParameterType == typeof(object[]);
    }

    public static bool IsParams(this ParameterInfo parameter)
    {
        return parameter.IsDefined(typeof(ParamArrayAttribute), false);
    }
    
    
    public static bool IsObjectArray(this Type type)
    {
        return !type.IsByRef &&
               type == typeof(object[]);
    }

    private static bool CanCast(Type argType, Type paramType)
    {
        if (argType.IsByRef || paramType.IsByRef)
            throw new NotImplementedException();
        if (argType == paramType) return true;
        if (argType == typeof(object) || paramType == typeof(object)) return true;
        if (argType.Implements(paramType)) return true;
        return false;
    }

    public static IEnumerable<ConstructorInfo> FindConstructors(Type instanceType, params Type[] argTypes)
    {
        return instanceType.GetConstructors((BindingFlags)Reflect.InstanceFlags)
                           // Limit to only ones with the same number of params
                           .Where(ctor =>
                           {
                               var ctorParams = ctor.GetParameters();
                               var len = ctorParams.Length;
                               if (len != argTypes.Length) return false;
                               for (var i = 0; i < len; i++)
                               {
                                   if (!CanCast(argTypes[i], ctorParams[i].ParameterType))
                                       return false;
                               }

                               return true;
                           }); }
}