using System.Diagnostics;
using Jay.Dumping;

namespace Jayflect.Dumping;

public sealed class ExpressionDumper : Dumper<Expression>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, 
        [NotNull] Expression expression, DumpFormat format)
    {
        switch (expression)
        {
            case ParameterExpression parameterExpression:
            {
                stringHandler.Dump(parameterExpression.Type, format);
                stringHandler.Write(' ');
                stringHandler.Write(parameterExpression.Name);
                return;
            }
            
            
            case BinaryExpression binaryExpression:
            {
                // Convert?
                if (binaryExpression.Conversion is not null)
                {
                    Debugger.Break();
                }
                Debugger.Break();
                return;
            }
            case BlockExpression blockExpression:
           
            case ConditionalExpression conditionalExpression:
           
            case ConstantExpression constantExpression:

            case DebugInfoExpression debugInfoExpression:

            case DefaultExpression defaultExpression:
           
            case DynamicExpression dynamicExpression:
            case GotoExpression gotoExpression:
            case IndexExpression indexExpression:
            case InvocationExpression invocationExpression:
            case LabelExpression labelExpression:
            case LambdaExpression lambdaExpression:
           
            case ListInitExpression listInitExpression:
           
            case LoopExpression loopExpression:
          
            case MemberExpression memberExpression:
            case MemberInitExpression memberInitExpression:
            case MethodCallExpression methodCallExpression:
            case NewArrayExpression newArrayExpression:
          
            case NewExpression newExpression:
        
            
          
            case RuntimeVariablesExpression runtimeVariablesExpression:
         
            case SwitchExpression switchExpression:
   
            case TryExpression tryExpression:
   
            case TypeBinaryExpression typeBinaryExpression:
      
            case UnaryExpression unaryExpression:
           
            default:
            {
                var exprType = expression.GetType();
                Debugger.Break();
                throw new NotImplementedException($"{expression.GetType()} has not yet been implemented");
            }
        }
    }
}