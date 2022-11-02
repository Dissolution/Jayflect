using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Serialization;

public abstract class Part : IDumpable
{
    public abstract void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat format = default);

    public sealed override string ToString()
    {
        return Dump(this);
    }
}

public class ComplexPart : Part
{
    public List<MemberPart> Members { get; }

    public ComplexPart(IEnumerable<MemberPart> children)
    {
        this.Members = children as List<MemberPart> ?? children.ToList();
    }

    public override void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat format = default)
    {
        //dumpHandler.Write(Members.Count);
        //dumpHandler.Write(" Members {");
        dumpHandler.Write("{");
        dumpHandler.WriteLine();
        foreach (var member in this.Members)
        {
            member.DumpTo(ref dumpHandler, format);
            dumpHandler.WriteLine();
        }
        dumpHandler.Write("}");
    }
}

public abstract class ValuePart : Part
{
    public abstract object? GetValue();
}

public class ConstValuePart : ValuePart
{
    private readonly object? _value;
    
    public ConstValuePart(object? value)
    {
        _value = value;
    }

    public override void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat format = default)
    {
        dumpHandler.Dump<object?>(_value, format);
    }

    public override object? GetValue()
    {
        return _value;
    }
}

public class GetValuePart : ValuePart
{
    private object? _instance;
    private readonly GetValue<object?, object> _getValue;

    public GetValuePart(object? instance,
        GetValue<object?, object> getValue)
    {
        _instance = instance;
        _getValue = getValue;
    }

    public override void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat format = default)
    {
        dumpHandler.Dump<object?>(GetValue(), format);
    }

    public override object? GetValue()
    {
        return _getValue(ref _instance);
    }
}

public sealed class NullValuePart : ConstValuePart
{
    public static NullValuePart Instance { get; } = new();
    
    public NullValuePart() : base(null)
    {
    }
}

public class MemberPart : Part
{
    public required string Name { get; init; }
    public required Type Type { get; init; }
    public required Part Value { get; init; }

    public override void DumpTo(ref DumpStringHandler dumpHandler, DumpFormat format = default)
    {
        dumpHandler.Dump(Type, format);
        dumpHandler.Write(' ');
        dumpHandler.Write(Name);
        dumpHandler.Write(": ");
        Value.DumpTo(ref dumpHandler, format);
    }
}


public static class ChopShop
{
    private static Part GetValueNode<T>(T? value)
    {
        if (value is null) return NullValuePart.Instance;
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>() || typeof(T) == typeof(string))
            return new ConstValuePart(value);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var memberParts = new List<MemberPart>();
        foreach (var property in properties)
        {
            var memberPart = new MemberPart
            {
                Name = property.Name,
                Type = property.PropertyType,
                Value = new GetValuePart(value, Reflect.GetGetValue<object?, object>(property)),
            };
            memberParts.Add(memberPart);
        }
        return new ComplexPart(memberParts);
    }
    
    public static MemberPart PartOut<T>(T? value, [CallerArgumentExpression(nameof(value))] string valueName = "")
    {
        var memberPart = new MemberPart
        {
            Name = valueName,
            Type = value?.GetType() ?? typeof(T),
            Value = GetValueNode<T>(value),
        };
        var str = memberPart.ToString();
        return memberPart;
    }
}
