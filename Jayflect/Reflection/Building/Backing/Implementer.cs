using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public abstract class Implementer
{
    protected internal static CallingConventions GetCallingConventions(MemberInfo member)
    {
        if (member.IsStatic()) return CallingConventions.Standard;
        return CallingConventions.HasThis;
    }

    protected internal static MethodAttributes GetMethodImplementationAttributes(MethodBase method)
    {
        var attr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final;
        if (method.IsStatic)
            attr |= MethodAttributes.Static;
        return attr;
    }

    protected readonly TypeBuilder _typeBuilder;

    protected Implementer(TypeBuilder typeBuilder)
    {
        _typeBuilder = typeBuilder;
    }
}