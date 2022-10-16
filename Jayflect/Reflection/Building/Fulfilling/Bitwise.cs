/*

    public static class Bitwise
    {
        public static readonly Operator Complement = new Operator(Group.Bitwise, Targets.Unary, "~", "op_OnesComplement", 0);
        public static readonly Operator LeftShift = new Operator(Group.Bitwise, Targets.Binary, "<<", "op_LeftShift", 3);
        public static readonly Operator RightShift = new Operator(Group.Bitwise, Targets.Binary, ">>", "op_RightShift", 3);
        public static readonly Operator And = new Operator(Group.Bitwise, Targets.Binary, "&", "op_BitwiseAnd", 6);
        public static readonly Operator Or = new Operator(Group.Bitwise, Targets.Binary, "|", "op_BitwiseOr", 8);
        public static readonly Operator Xor = new Operator(Group.Bitwise, Targets.Binary, "^", "op_ExclusiveOr", 7);
    }

    public static readonly Operator Increment = new Operator(Group.Arithmetic, Targets.Unary, "++", "op_Increment", 0);
    public static readonly Operator Decrement = new Operator(Group.Arithmetic, Targets.Unary, "--", "op_Decrement", 0);
    public static readonly Operator Plus = new Operator(Group.Arithmetic, Targets.Unary, "+", "op_UnaryPlus", 0);
    public static readonly Operator Minus = new Operator(Group.Arithmetic, Targets.Unary, "-", "op_UnaryNegation", 0);
    public static readonly Operator Multiplication = new Operator(Group.Arithmetic, Targets.Binary, "*", "op_Multiply", 1);
    public static readonly Operator Division = new Operator(Group.Arithmetic, Targets.Binary, "/", "op_Division", 1);
    public static readonly Operator Remainder = new Operator(Group.Arithmetic, Targets.Binary, "%", "op_Modulus", 1);
    public static readonly Operator Addition = new Operator(Group.Arithmetic, Targets.Binary, "+", "op_Addition", 2);
    public static readonly Operator Subtraction = new Operator(Group.Arithmetic, Targets.Binary, "-", "op_Subtraction", 2);

    public static readonly Operator LessThan = new Operator(Group.Comparison, Targets.Binary, "<", "op_LessThan", 4);
    public static readonly Operator GreaterThan = new Operator(Group.Comparison, Targets.Binary, ">", "op_GreaterThan", 4);
    public static readonly Operator LessThanOrEqual = new Operator(Group.Comparison, Targets.Binary, "<=", "op_LessThanOrEqual", 4);
    public static readonly Operator GreaterThanOrEqual = new Operator(Group.Comparison, Targets.Binary, ">", "op_GreaterThanOrEqual", 4);

    public static readonly Operator Equality = new Operator(Group.Equality, Targets.Binary, "==", "op_Equality", 5);
    public static readonly Operator Inequality = new Operator(Group.Equality, Targets.Binary, "!=", "op_Inequality", 5);

    public static readonly Operator Implicit =
        new Operator(Fulfilling.Group.Other, Targets.Binary, "(implicit)", "op_Implicit", -1);
    public static readonly Operator Explicit =
        new Operator(Fulfilling.Group.Other, Targets.Binary, "(explicit)", "op_Explicit", -1);

    /* op_True
     * op_False
     #2#

    private readonly int _priority;

    public Group Group { get; }
    public Targets Targets { get; }
    public string Symbol { get; }
    public string MetadataName { get; }
    public DelegateSig Signature { get; }

    private Operator(Group group, Targets targets, string symbol, 
                     string metadataName,
                     int priority, 
                     DelegateSig signature,
                     [CallerMemberName] string name = "")
        : base(name)
    {
        this.Group = group;
        this.Targets = targets;
        this.Symbol = symbol;
        this.MetadataName = metadataName;
        this.Signature = signature;
        _priority = priority;
    }

    public override int CompareTo(Operator? op)
    {
        if (op is null) return 1;
        return _priority.CompareTo(op._priority);
    }
}

public static class TypeOperatorCache
{
    public sealed class OperatorCache
    {
        private readonly ConcurrentDictionary<Operator, Delegate?> _operatorCache;

        public Type Type { get; }

        internal OperatorCache(Type type)
        {
            this.Type = type;
            _operatorCache = new ConcurrentDictionary<Operator, Delegate?>();
        }

        internal Result TryEmit(IILGeneratorEmitter emitter,
                                Operator @operator,
                                Type argType)
        {
            // Func<T,T>
            if (@operator.Targets == Targets.Unary)
            {
                // Special

            }
        }

        internal Result TryGetDelegate<TDelegate>(Operator @operator)
            where TDelegate : Delegate
        {
            var delSig = DelegateSig.Of<TDelegate>();
            if (@operator.Targets == Targets.Unary)
            {
                if (delSig.ParameterCount != 1)
                {
                    return Dumper.GetException<ArgumentException>($"{typeof(TDelegate)} is not valid for Unary Operand {@operator}", nameof(@operator));
                }
                var returnType = delSig.ReturnType;
                if (returnType != delSig.ParameterTypes[0])
                {
                    return Dumper.GetException<ArgumentException>($"{typeof(TDelegate)} is not valid for Unary Operand {@operator}", nameof(@operator));
                }
                
            }
        }
    }

    private static readonly ConcurrentTypeDictionary<ConcurrentDictionary<Operator, Delegate?>> _operatorCache;

    static TypeOperatorCache()
    {

    }
}
*/