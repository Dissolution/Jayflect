using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jay.Dumping;



/// <summary>
/// A shared base class for <see cref="Dumper{T}"/> instances
/// </summary>
public abstract class Dumper : IDumper
{
    protected internal static bool DumpedNull<T>(ref DumpStringHandler stringHandler, [AllowNull, NotNullWhen(false)] T? value, DumpFormat dumpFormat)
    {
        if (value is null)
        {
            if (dumpFormat == DumpFormat.View)
            {
                stringHandler.Write("null");
            }
            else if (dumpFormat >= DumpFormat.Inspect)
            {
                stringHandler.Write("(");
                stringHandler.Dump(typeof(T));
                stringHandler.Write(")null");
            }
            return true;
        }
        return false;
    }


    /// <summary>
    /// Can this <see cref="Dumper"/> dump values of the given <paramref name="type"/>?
    /// </summary>
    public abstract bool CanDump(Type type);

    public abstract void DumpTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat dumpFormat = default);
}