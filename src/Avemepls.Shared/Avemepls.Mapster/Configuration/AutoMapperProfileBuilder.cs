using System.Linq.Expressions;

using Mapster;

namespace Avemepls.Mapster.Configuration;

#pragma warning disable S1694
public abstract class AutoMapperProfileBuilder
#pragma warning restore S1694
{
    internal abstract void Build(TypeAdapterConfig config);
}

#pragma warning disable SA1402
public class AutoMapperProfileBuilder<TSource, TDestination> : AutoMapperProfileBuilder
#pragma warning restore SA1402
{
    private readonly List<Action<TypeAdapterSetter<TSource, TDestination>>> _configurators = [];

    public AutoMapperProfileBuilder<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember>> configureMap)
    {
        var propertyBuilder =
            new AutoMapperMemberConfigurationExpression<TSource, TDestination, TMember>(destinationMember);

        configureMap.Invoke(propertyBuilder);
        _configurators.Add(config => propertyBuilder.Build(config));

        return this;
    }

    public AutoMapperProfileBuilder<TSource, TDestination> AfterMap(Action<TSource, TDestination> mapper)
    {
        _configurators.Add(config => config.AfterMapping(mapper));

        return this;
    }

    public AutoMapperProfileBuilder<TSource, TDestination> ReverseMap()
    {
        _configurators.Add(config => config.TwoWays());

        return this;
    }

    public AutoMapperProfileBuilder<TSource, TDestination> Configure(Action<TypeAdapterSetter<TSource, TDestination>> configure)
    {
        _configurators.Add(configure);

        return this;
    }

    public AutoMapperProfileBuilder<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
    {
        _configurators.Add(config => config.Inherits<TSourceBase, TDestinationBase>());

        return this;
    }

    public AutoMapperProfileBuilder<TSource, TDestination> MaxDepth(
        int count)
    {
        _configurators.Add(config => config.MaxDepth(count));

        return this;
    }

    internal override void Build(TypeAdapterConfig config)
    {
        var typeConfig = config.ForType<TSource, TDestination>();

        foreach (var configurator in _configurators)
        {
            configurator.Invoke(typeConfig);
        }
    }
}