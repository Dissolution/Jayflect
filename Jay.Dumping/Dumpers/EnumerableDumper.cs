using System.Collections;
using Jay.Dumping.Interpolated;
using Jay.Extensions;

namespace Jay.Dumping;

[DumpOptions(Priority = 8)]
public sealed class EnumerableDumper : Dumper<IEnumerable>
{
    public override bool CanDump(Type type)
    {
        return type != typeof(string) &&
               type.Implements<IEnumerable>();
    }

    protected override void DumpImpl(ref DumpStringHandler dumpHandler, 
        [DisallowNull] IEnumerable enumerable, DumpFormat format)
    {
        if (format.IsWithType)
        {
            dumpHandler.Dump(enumerable.GetType());
        }
        
        // Individual values
        dumpHandler.Write('{');
        ReadOnlySpan<char> delimiter = ", ";
        if (format.IsCustom)
        {
            delimiter = format;
            format = default;
        }
        var objDumper = DumperCache.GetDumper<object>();
        if (enumerable is ICollection collection)
        {
            if (enumerable is IList list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (i > 0) dumpHandler.Write(delimiter);
                    objDumper.DumpTo(ref dumpHandler, list[i], format);
                }
                dumpHandler.Write('}');
                return;
            }
            
            if (collection.Count == 0)
            {
                dumpHandler.Write('}');
                return;
            }
        }
       
        
        IEnumerator? enumerator = null;
        try
        {
            enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext()) return;
            objDumper.DumpTo(ref dumpHandler, enumerator.Current, format);
            while (enumerator.MoveNext())
            {
                dumpHandler.Write(delimiter);
                objDumper.DumpTo(ref dumpHandler, enumerator.Current, format);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
                disposable.Dispose();
        }

        dumpHandler.Write('}');
    }
}