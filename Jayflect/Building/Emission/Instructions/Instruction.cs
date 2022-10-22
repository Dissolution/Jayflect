using Jay.Comparison;
using Jay.Dumping;

namespace Jayflect.Building.Emission.Instructions;

public abstract class Instruction : IEquatable<Instruction>, IDumpable
{
    static Instruction()
    {
        DefaultComparers.Replace<LocalBuilder>(new LocalBuilderEqualityComparer());
    }
    
    public abstract bool Equals(Instruction? instruction);

    public abstract void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default);
}