using System.Linq.Expressions;
using System.Reflection;

namespace Avemepls.Core.Extensions;

public static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<TSource>(this LambdaExpression propertyLambda)
    {
        var type = typeof(TSource);

        if (propertyLambda.Body is not MemberExpression member)
        {
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
        }

        if (member.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
        }

        if (type != propInfo.ReflectedType
            && propInfo.ReflectedType is not null
            && !type.IsSubclassOf(propInfo.ReflectedType))
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda,
                type));
        }

        return propInfo;
    }

    public static PropertyInfo GetPropertyInfo(this LambdaExpression propertyLambda)
    {
        if (propertyLambda.Body is not MemberExpression member)
        {
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
        }

        if (member.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
        }

        return propInfo;
    }
}