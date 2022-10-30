using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Building.Emission;

public sealed class EmitterLocal : IEquatable<EmitterLocal>, IEquatable<LocalBuilder>, IDumpable
{
    public static implicit operator LocalBuilder(EmitterLocal local) => local._localBuilder;

    public static bool operator ==(EmitterLocal left, EmitterLocal right) =>
        LocalBuilderEqualityComparer.Default.Equals(left._localBuilder, right._localBuilder);
    public static bool operator !=(EmitterLocal left, EmitterLocal right) =>
        !LocalBuilderEqualityComparer.Default.Equals(left._localBuilder, right._localBuilder);
    public static bool operator ==(EmitterLocal left, LocalBuilder right) =>
        LocalBuilderEqualityComparer.Default.Equals(left._localBuilder, right);
    public static bool operator !=(EmitterLocal left, LocalBuilder right) =>
        !LocalBuilderEqualityComparer.Default.Equals(left._localBuilder, right);
    
    private LocalBuilder _localBuilder;
    private string _name;

    public LocalBuilder Local
    {
        get => _localBuilder;
        internal set => _localBuilder = value;
    }

    public string Name
    {
        get => _name;
        internal set => _name = value;
    }

    public int Index => _localBuilder.LocalIndex;
    public Type Type => _localBuilder.LocalType;
    public bool IsPinned => _localBuilder.IsPinned;
    public bool IsShortForm => _localBuilder.IsShortForm();

    public EmitterLocal(LocalBuilder local, string? name = null)
    {
        _localBuilder = local;
        if (string.IsNullOrWhiteSpace(name))
        {
            _name = $"Local{local.LocalIndex}";
        }
        else
        {
            _name = name;
        }
    }

    public bool Equals(EmitterLocal? emitterLocal)
    {
        return Equals(_localBuilder, emitterLocal?._localBuilder) &&
               string.Equals(_name, emitterLocal._name);
    }

    public bool Equals(LocalBuilder? localBuilder)
    {
        return localBuilder is not null &&
               localBuilder.LocalIndex == _localBuilder.LocalIndex &&
               localBuilder.LocalType == _localBuilder.LocalType &&
               localBuilder.IsPinned == _localBuilder.IsPinned;
    }

    public override bool Equals(object? obj)
    {
        if (obj is EmitterLocal emitterLocal) return Equals(emitterLocal);
        if (obj is LocalBuilder localBuilder) return Equals(localBuilder);
        return false;
    }
    public override int GetHashCode()
    {
        return Index;
    }

    public void DumpTo(ref DumpStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        // Declare or Use, defaults to Use
        if (dumpFormat == "D")
        {
            // [#] (pinned) type Name
            stringHandler.Write('[');
            stringHandler.Write(Index);
            stringHandler.Write("] ");
            if (_localBuilder.IsPinned)
            {
                stringHandler.Write("pinned ");
            }
            stringHandler.Dump(Type);
            stringHandler.Write(' ');
            stringHandler.Write(Name);
        }
        else
        {
            stringHandler.Write(Name);
        }
    }
    
    public override string ToString()
    {
        return Name;
    }
}