using Mapster;

namespace Avemepls.Mapster.Configuration;

public class AutoMapperProfileBuilderGeneric : AutoMapperProfileBuilder
{
    private readonly Type _source;
    private readonly Type _destination;

    private readonly IList<AutoMapperMemberConfigurationExpressionBuilder> _propertyBuilders =
        new List<AutoMapperMemberConfigurationExpressionBuilder>();

    private readonly List<Action<TypeAdapterSetter>> _configurators = [];

    public AutoMapperProfileBuilderGeneric(Type source, Type destination)
    {
        _source = source;
        _destination = destination;
    }

    public AutoMapperProfileBuilderGeneric ForMember(
        string destinationMember,
        Action<AutoMapperMemberConfigurationExpressionGeneric> configureMap)
    {
        var propertyBuilder =
            new AutoMapperMemberConfigurationExpressionGeneric(destinationMember);

        configureMap.Invoke(propertyBuilder);
        _configurators.Add(config => propertyBuilder.Build(config));

        return this;
    }

    public AutoMapperProfileBuilderGeneric MaxDepth(
        int count)
    {
        _configurators.Add(config => config.MaxDepth(count));

        return this;
    }

    internal override void Build(TypeAdapterConfig config)
    {
        var typeConfig = config.ForType(_source, _destination)
            .ShallowCopyForSameType(true);

        foreach (var propertyBuilder in _propertyBuilders)
        {
            propertyBuilder.Build(typeConfig);
        }
    }
}