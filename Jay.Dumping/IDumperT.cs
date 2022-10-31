using Jay.Dumping.Interpolated;

namespace Jay.Dumping;

public interface IDumper<in T> : IDumper
{
    void DumpTo(ref DumpStringHandler dumpHandler, T? value, DumpFormat format = default);
}