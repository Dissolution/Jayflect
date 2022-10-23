using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jay.Dumping;

public abstract class Dumper<T> : Dumper
{
    public sealed override bool CanDump(Type type) => type.Implements<T>();

    internal sealed override void DumpObject(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] object obj, DumpFormat dumpFormat)
    {
        if (obj is T value)
        {
            DumpValue(ref stringHandler, value, dumpFormat);
        }
        throw new ArgumentException($"The given object '{obj}' is not a {typeof(T)} value");
    }
    
    protected abstract void DumpValueImpl(ref DefStringHandler stringHandler, [NotNull] T value, DumpFormat dumpFormat);

    protected bool WroteNull(ref DefStringHandler stringHandler, [AllowNull, NotNullWhen(false)] T value, DumpFormat dumpFormat)
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
    
    public void DumpValue(ref DefStringHandler stringHandler, T? value, DumpFormat dumpFormat = default)
    {
        if (WroteNull(ref stringHandler, value, dumpFormat)) return;

        if (dumpFormat.IsCustom)
        {
            // pass-through
            stringHandler.AppendFormatted<T>(value, dumpFormat.GetCustomFormatString());
        }
        else
        {
            // call the implementation
            this.DumpValueImpl(ref stringHandler, value, dumpFormat);
        }
    }

    public override string ToString() => $"Dumper<{typeof(T).Name}>";
}