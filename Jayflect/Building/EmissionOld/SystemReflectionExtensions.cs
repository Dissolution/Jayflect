using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Emission;

internal static class SystemReflectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsShort(this Label label) => label.GetHashCode() <= sbyte.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsShort(this LocalBuilder local)
    {
        // if (local is null)
        //     throw new ArgumentNullException(nameof(local));
        // var index = local.LocalIndex;
        // if (index < 0 || index > 65534)
        //     throw new ArgumentOutOfRangeException(nameof(local), index, "LocalBuilder index must be between 0 and 65534");
        return local.LocalIndex <= byte.MaxValue;
    }

    public static OpCode GetCallOpCode(this MethodBase method)
    {
        // We always want to use Callvirt when in doubt, as it is the 'safest' option to get the behavior
        // that we're expecting. However, when we know that the method is non-virtual (sealed) we can use Call
        // which is slightly more performant.
        
        // Static methods are sealed, as are non-virtual methods
        if (method.IsStatic || !method.IsVirtual)
            return OpCodes.Call;
        
        // Value type methods are also sealed
        var source = method.ReflectedType ?? method.DeclaringType;
        if (source != null && source.IsValueType)
            return OpCodes.Call;
        
        return OpCodes.Callvirt;
    }


}