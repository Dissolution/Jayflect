/*using System.Linq.Expressions;
using Jay.Reflection.Caching;
using ExprType = System.Linq.Expressions.ExpressionType;

namespace Jay.Reflection.Building.Fulfilling;

public enum Group
{
    Arithmetic,
    Comparison,
    BooleanLogic,
    Bitwise,
    Equality,
    Other,
}

public enum Targets
{
    None = 0,
    Unary = 1,
    Binary = 2,
}

//https://stackoverflow.com/questions/11113259/how-to-call-custom-operator-with-reflection

/// <see cref="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/"/>
/// <see cref="https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads"/>
public sealed class Operator : IEquatable<Operator>, IComparable<Operator>
{
    private static readonly List<Operator> _operators;

    public static readonly Operator Not = new Operator
    {
        Priority = 0,
        Group = Group.BooleanLogic,
        Targets = Targets.Unary,
        Symbol = "!",
        MethodName = "op_LogicalNot",
        Signature = MethodSig.Of(typeof(Func<bool, bool>)),
        ExpressionType = ExprType.Not,
    };

    public static readonly Operator LogicalAnd = new Operator
    {
        Priority = 6,
        Group = Group.BooleanLogic,
        Targets = Targets.Binary,
        Symbol = "&",
        MethodName = "op_LogicalAnd",
        Signature = MethodSig.Of(typeof(Func<,,>)),
        ExpressionType = ExprType.And,
    };

    public static readonly Operator LogicalOr = new Operator
    {
        Priority = 8,
        Group = Group.BooleanLogic,
        Targets = Targets.Binary,
        Symbol = "|",
        MethodName = "op_LogicalOr",
        Signature = MethodSig.Of(typeof(Func<,,>)),
        ExpressionType = ExprType.Or,
    };

    public static readonly Operator Xor = new Operator
    {
        Priority = 7,
        Group = Group.BooleanLogic,
        Targets = Targets.Binary,
        Symbol = "^",
        MethodName = "op_ExclusiveOr",
        Signature = MethodSig.Of(typeof(Func<,,>)),
        ExpressionType = ExprType.ExclusiveOr,
    };

    public static readonly Operator ConditionalAnd = new Operator
    {
        Priority = 9,
        Group = Group.BooleanLogic,
        Targets = Targets.Binary,
        Symbol = "&&",
        MethodName = "op_LogicalAnd",
        Signature = MethodSig.Of(typeof(Func<,,>)),
        ExpressionType = ExprType.AndAlso,
    };


    public static readonly Operator ConditionalOr = new Operator
    {
        Priority = 10,
        Group = Group.BooleanLogic,
        Targets = Targets.Binary,
        Symbol = "||",
        MethodName = "op_LogicalOr",
        Signature = MethodSig.Of(typeof(Func<,,>)),
        ExpressionType = ExprType.OrElse,
    };


    static Operator()
    {
        _operators = new List<Operator>(85);    // ExpressionType.Count
    }

    public static bool TryParse(string? text, out Operator? @operator)
    {
        throw new NotImplementedException();
    }


    private int Priority { get; init; }

    public string Name { get; }

    public Group Group { get; init; }
    public Targets Targets { get; init; }
    public string Symbol { get; init; }

    public string MethodName { get; init; }
    public MethodSig Signature { get; init; }

    public ExpressionType? ExpressionType { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Operator([CallerMemberName] string name = "")
    {
        this.Name = name;
        _operators.Add(this);
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public int CompareTo(Operator? @operator)
    {
        if (@operator is null) return 1;
        return Priority.CompareTo(@operator.Priority);
    }

    public bool Equals(Operator? @operator)
    {
        return ReferenceEquals(this, @operator);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return Hasher.Create(Name);
    }

    public override string ToString()
    {
        return $"Name ({Symbol})";
    }
}*/