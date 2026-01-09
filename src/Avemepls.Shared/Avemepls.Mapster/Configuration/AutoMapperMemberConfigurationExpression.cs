using System.Linq.Expressions;
using System.Reflection;

using Mapster;
using Mapster.Models;

namespace Avemepls.Mapster.Configuration;

public class
    AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember>(Expression<Func<TDestination, TMember>> destinationMember) :
    AutoMapperMemberConfigurationExpressionBuilder
{
    private readonly List<Action<TypeAdapterSetter>> _configurators = [];
    private LambdaExpression _source;
    private LambdaExpression? _shouldMap;

    public AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember> MapFrom<TSourceMember>(
        Expression<Func<TSource, TSourceMember>> source)
    {
        _source = source;

        return this;
    }

    public AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember> Condition<TSourceMember>(
        Expression<Func<TSource, TSourceMember>> condition)
    {
        _shouldMap = condition;

        return this;
    }

    private bool _isIgnored;

    public AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember> Ignore()
    {
        _configurators.Add(config => config.Ignore(GetMemberPath(destinationMember)!));
        _isIgnored = true;

        return this;
    }

    internal override void Build(TypeAdapterSetter config)
    {
        if (_isIgnored)
        {
            foreach (var configurator in _configurators)
            {
                configurator.Invoke(config);
            }
        }
        else
        {
            var sourceName = GetMemberPath(_source, noError: true);

            config.Settings.Resolvers.Add(new InvokerModel
            {
                DestinationMemberName = GetMemberPath(destinationMember)!,
                SourceMemberName = sourceName,
                Invoker = _source,
                Condition = _shouldMap
            });
        }
    }

    private static string? GetMemberPath(LambdaExpression lambda, bool firstLevelOnly = false, bool noError = false)
    {
        var props = new List<string>();
        var expr = TrimConversion(lambda.Body, true);

        while (expr?.NodeType == ExpressionType.MemberAccess)
        {
            if (firstLevelOnly && props.Count > 0)
            {
                if (noError)
                    return null;

                throw new ArgumentException("Only first level members are allowed (eg. obj => obj.Child)",
                                            nameof(lambda));
            }

            var memEx = (MemberExpression)expr;
            props.Add(memEx.Member.Name);
            expr = memEx.Expression;
        }

        if (props.Count == 0 || expr?.NodeType != ExpressionType.Parameter)
        {
            if (noError)
                return null;

            throw new ArgumentException("Allow only member access (eg. obj => obj.Child.Name)", nameof(lambda));
        }

        props.Reverse();

        return string.Join(".", props);
    }

    private static Expression TrimConversion(Expression exp, bool force = false)
    {
        while (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked)
        {
            var unary = (UnaryExpression)exp;

            if (force || IsReferenceAssignableFrom(unary.Type, unary.Operand.Type))
                exp = unary.Operand;
            else
                break;
        }

        return exp;
    }

    private static bool IsReferenceAssignableFrom(Type destType, Type srcType)
    {
        if (destType == srcType)
            return true;

        if (!destType.GetTypeInfo().IsValueType && !srcType.GetTypeInfo().IsValueType &&
            destType.GetTypeInfo().IsAssignableFrom(srcType.GetTypeInfo()))
            return true;

        return false;
    }
}