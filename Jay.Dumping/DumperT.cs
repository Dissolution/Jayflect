using Jay.Dumping.Extensions;
using Jay.Dumping.Interpolated;
using Jay.Extensions;

namespace Jay.Dumping;

public abstract class Dumper<T> : Dumper, IDumper<T>
{
    public override bool CanDump(Type type) => type.Implements<T>();
    
    protected abstract void DumpImpl(ref DumpStringHandler dumpHandler, [DisallowNull] T value, DumpFormat format);

    public override void DumpObjTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat format = default)
    {
        if (obj is T value)
        {
            DumpTo(ref dumpHandler, value, format);
        }
        // Can we pass null to the DumpTo<T> handler?
        else if (obj is null && !typeof(T).IsValueType)
        {
            Debug.WriteLine("Dumper<T>.DumpTo(obj) passed (object)null to DumpTo<T>");
            DumpTo(ref dumpHandler, default(T), format);
        }
        // Failed (should probably never get here)
        else
        {
            var message = DumpingExtensions.Dump($"The given object '{obj}' is not a {typeof(T)} value");
            throw new ArgumentException(message, nameof(obj));
        }
    }

    public virtual void DumpTo(ref DumpStringHandler dumpHandler, T? value, DumpFormat format = default)
    {
        if (DumpedNull<T>(ref dumpHandler, value, format)) return;

        if (format.IsCustom)
        {
            // pass-through
            dumpHandler.Write<T>(value, format.GetCustomFormatString());
        }
        else
        {
            // call the implementation
            this.DumpImpl(ref dumpHandler, value, format);
        }
    }

    public override string ToString() => Dump($"Dumper<{typeof(T)}>");
}