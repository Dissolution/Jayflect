/*namespace Jayflect.Expressions;

// https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expressiontype?view=net-7.0
public static class ExpressionHelper
{
    public class ExpressionInfo
    {
        public ExpressionType ExpressionType { get; init; }
        
        public string Symbol { get; init; }
        
        public bool Checked { get; init; } = false;

        public bool Numeric { get; init; } = false;

        public bool Bitwise { get; init; } = false;

        public bool Logical { get; init; } = false;

        public Type? InterfaceType { get; set; } = null;

        public DelegateInfo? DelegateInfo { get; set; } = null;
        
        public ExpressionInfo(ExpressionType expressionType)
        {
            this.ExpressionType = expressionType;
        }
    }
    

    internal static Type GenericType = typeof(IList<>).GenericTypeArguments[0];
    
    public static ExpressionInfo GetInfo(ExpressionType expressionType)
    {
        switch (expressionType)
        {

            case ExpressionType.Add:
                return new(expressionType)
                {
                    Symbol = "+",
                    Numeric = true,
                };
            case ExpressionType.AddAssign:
                return new(expressionType)
                {
                    Symbol = "+=",
                    Numeric = true,
                };
            case ExpressionType.AddAssignChecked:
                return new(expressionType)
                {
                    Symbol = "+=",
                    Numeric = true,
                    Checked = true,
                };
            case ExpressionType.AddChecked:
                return new(expressionType)
                {
                    Symbol = "+",
                    Numeric = true,
                    Checked = true,
                };
            case ExpressionType.And:
                return new(expressionType)
                {
                    Symbol = "&",
                    Bitwise = true,
                    Logical = true,
                    DelegateInfo = new()
                    {
                        Name = "op_BitwiseAnd",
                        ReturnType = GenericType,
                        ParameterTypes = new[]{GenericType, GenericType},
                    },
                };
            case ExpressionType.AndAlso:
                return new(expressionType)
                {
                    Symbol = "&&",
                    Logical = true,
                };
            case ExpressionType.AndAssign:
                return new(expressionType)
                {
                    Symbol = "&=",
                    Bitwise = true,
                    Logical = true,
                };
            case ExpressionType.ArrayIndex:
                return new(expressionType)
                {
                    Symbol = "array[index]",
                    DelegateInfo = new("??", GenericType, typeof(Array), GenericType),
                };
            case ExpressionType.ArrayLength:
                return new(expressionType)
                {
                    Symbol = "array.Length",
                    DelegateInfo = new("??", typeof(int), typeof(Array)),
                };
            case ExpressionType.Assign:
                return new(expressionType)
                {
                    Symbol = "=",
                };
            case ExpressionType.Block:
                return new(expressionType)
                {
                    Symbol = "[Expressions]",
                };
            case ExpressionType.Call:
                return new(expressionType)
                {
                    Symbol = "()",
                    DelegateInfo = new("Invoke", GenericType),
                };
            case ExpressionType.Coalesce:
                return new(expressionType)
                {
                    Symbol = "??",
                    DelegateInfo = new(null!, GenericType, GenericType, GenericType),
                };
            case ExpressionType.Conditional:
                return new(expressionType)
                {
                    Symbol = "?:",
                    DelegateInfo = new(null!, GenericType, typeof(Predicate<>), GenericType, GenericType),
                };
            case ExpressionType.Constant:
                return new(expressionType)
                {
                    Symbol = "const",
                    DelegateInfo = new(null!, GenericType),
                };
            case ExpressionType.Convert:
                return new(expressionType)
                {
                    Symbol = "(Type)value",
                    Checked = false,
                    DelegateInfo = new(null!, GenericType, GenericType),
                };
            case ExpressionType.ConvertChecked:
                return new(expressionType)
                {
                    Symbol = "(Type)value",
                    Checked = true,
                    DelegateInfo = new(null!, GenericType, GenericType),
                };
            case ExpressionType.DebugInfo:
                return new(expressionType)
                {
                    Symbol = "Debug",
                };
            case ExpressionType.Divide:
                break;
            case ExpressionType.Equal:
                break;
            case ExpressionType.ExclusiveOr:
                break;
            case ExpressionType.GreaterThan:
                break;
            case ExpressionType.GreaterThanOrEqual:
                break;
            case ExpressionType.Invoke:
                break;
            case ExpressionType.Lambda:
                break;
            case ExpressionType.LeftShift:
                break;
            case ExpressionType.LessThan:
                break;
            case ExpressionType.LessThanOrEqual:
                break;
            case ExpressionType.ListInit:
                break;
            case ExpressionType.MemberAccess:
                break;
            case ExpressionType.MemberInit:
                break;
            case ExpressionType.Modulo:
                break;
            case ExpressionType.Multiply:
                break;
            case ExpressionType.MultiplyChecked:
                break;
            case ExpressionType.Negate:
                break;
            case ExpressionType.UnaryPlus:
                break;
            case ExpressionType.NegateChecked:
                break;
            case ExpressionType.New:
                break;
            case ExpressionType.NewArrayInit:
                break;
            case ExpressionType.NewArrayBounds:
                break;
            case ExpressionType.Not:
                break;
            case ExpressionType.NotEqual:
                break;
            case ExpressionType.Or:
                break;
            case ExpressionType.OrElse:
                break;
            case ExpressionType.Parameter:
                break;
            case ExpressionType.Power:
                break;
            case ExpressionType.Quote:
                break;
            case ExpressionType.RightShift:
                break;
            case ExpressionType.Subtract:
                break;
            case ExpressionType.SubtractChecked:
                break;
            case ExpressionType.TypeAs:
                break;
            case ExpressionType.TypeIs:
                break;



            case ExpressionType.Decrement:
                break;
            case ExpressionType.Dynamic:
                break;
            case ExpressionType.Default:
                break;
            case ExpressionType.Extension:
                break;
            case ExpressionType.Goto:
                break;
            case ExpressionType.Increment:
                break;
            case ExpressionType.Index:
                break;
            case ExpressionType.Label:
                break;
            case ExpressionType.RuntimeVariables:
                break;
            case ExpressionType.Loop:
                break;
            case ExpressionType.Switch:
                break;
            case ExpressionType.Throw:
                break;
            case ExpressionType.Try:
                break;
            case ExpressionType.Unbox:
                break;

            
            

            case ExpressionType.DivideAssign:
                break;
            case ExpressionType.ExclusiveOrAssign:
                break;
            case ExpressionType.LeftShiftAssign:
                break;
            case ExpressionType.ModuloAssign:
                break;
            case ExpressionType.MultiplyAssign:
                break;
            case ExpressionType.OrAssign:
                break;
            case ExpressionType.PowerAssign:
                break;
            case ExpressionType.RightShiftAssign:
                break;
            case ExpressionType.SubtractAssign:
                break;

            case ExpressionType.MultiplyAssignChecked:
                break;
            case ExpressionType.SubtractAssignChecked:
                break;
            case ExpressionType.PreIncrementAssign:
                break;
            case ExpressionType.PreDecrementAssign:
                break;
            case ExpressionType.PostIncrementAssign:
                break;
            case ExpressionType.PostDecrementAssign:
                break;
            case ExpressionType.TypeEqual:
                break;
            case ExpressionType.OnesComplement:
                break;
            case ExpressionType.IsTrue:
                break;
            case ExpressionType.IsFalse:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expressionType), expressionType, null);
        }

        throw new NotImplementedException();
    }
}*/