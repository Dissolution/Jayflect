using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jay.Dumping;

public abstract class Dumper<T> : Dumper, IDumper<T>
{
    public override bool CanDump(Type type) => type.Implements<T>();

    public override void DumpTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat dumpFormat = default)
    {
        if (obj is null)
        {
            return;
        }
        if (obj is T value)
        {
            DumpTo(ref dumpHandler, value, dumpFormat);
            return;
        }
        // *weird* issues
        // try
        // {
        //     value = (T)obj;
        //     Dump(ref stringHandler, value, dumpFormat);
        //     Debug.WriteLine($"Had to force object to {typeof(T).Name}");
        //     return;
        // }
        // catch (Exception ex)
        // {
        //     // ignored
        //     Debug.WriteLine($"Object forcing failed: {ex}");
        // }

        throw new ArgumentException($"The given object '{obj}' is not a {typeof(T)} value");
    }
    
    protected abstract void DumpImpl(ref DumpStringHandler dumpHandler, [NotNull] T value, DumpFormat format);
  
    public void DumpTo(ref DumpStringHandler dumpHandler, T? value, DumpFormat dumpFormat = default)
    {
        if (DumpedNull(ref dumpHandler, value, dumpFormat)) return;

        if (dumpFormat.IsCustom)
        {
            // pass-through
            dumpHandler.AppendFormatted<T>(value, dumpFormat.GetCustomFormatString());
        }
        else
        {
            // call the implementation
            this.DumpImpl(ref dumpHandler, value, dumpFormat);
        }
    }

    public override string ToString() => $"Dumper<{typeof(T).Name}>";
}