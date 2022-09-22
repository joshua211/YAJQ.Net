using System.Linq.Expressions;

namespace YAJQ.Core.JobQueue;

public class FieldToConstantArgumentVisitor : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var argumentExpressions = new List<Expression>();
        foreach (var argument in node.Arguments)
        {
            var argumentResolver = Expression.Lambda(argument);
            var argumentValue = argumentResolver.Compile().DynamicInvoke();

            var valueExpression = Expression.Constant(argumentValue, argument.Type);
            argumentExpressions.Add(valueExpression);
        }

        return Expression.Call(node.Object, node.Method, argumentExpressions);
    }
}