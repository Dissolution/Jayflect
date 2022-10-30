using Jay.Dumping.Extensions;
using Jay.Extensions;

namespace Jay.Dumping;

public interface IDumper<in T> : IDumper
{
    bool IDumper.CanDump(Type type) => type.Implements<T>();
    
    void IDumper.DumpTo(ref DumpStringHandler dumpHandler, object? obj, DumpFormat format)
    {
        if (obj is T value)
        {
            DumpTo(ref dumpHandler, value, format);
        }
        else if (obj is null && !typeof(T).IsValueType)
        {
            Debug.WriteLine("IDumper<T> passed (object)null to Dump<T>");
            DumpTo(ref dumpHandler, default(T), format);
        }
        else
        {
            var message = DumpingExtensions.Dump($"The given object '{obj}' is not a {typeof(T)} value");
            throw new ArgumentException(message, nameof(obj));
        }
    }

    void DumpTo(ref DumpStringHandler dumpHandler, T? value, DumpFormat format = default);
}