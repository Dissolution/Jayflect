using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

/// <summary>
/// A shared base class for <see cref="Dumper{T}"/> instances
/// </summary>
public abstract class Dumper : IDumper
{
    protected internal static bool DumpedNull<T>(
        ref DumpStringHandler stringHandler, 
        [AllowNull, NotNullWhen(false)] T? value, 
        DumpFormat dumpFormat)
    {
        if (value is null)
        {
            if (dumpFormat.IsWithType)
            {
                stringHandler.Write("(");
                stringHandler.Dump(typeof(T));      // Not with type
                stringHandler.Write(")");
            }
            
            // The actual word `null`, to differentiate between whitespace
            stringHandler.Write("null");
            
            return true;
        }
        return false;
    }


    /// <inheritdoc cref="IDumper"/>
    public abstract bool CanDump(Type type);

    /// <inheritdoc cref="IDumper"/>
    public abstract void DumpObjTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat dumpFormat = default);

    public override string ToString()
    {
        return "Dumper";
    }
}