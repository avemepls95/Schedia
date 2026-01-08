using Mapster;

namespace Avemepls.Mapster.Configuration;

/// <summary>
/// Mapster как AutoMapper
/// </summary>
public abstract class Profile : IRegister
{
    private readonly List<AutoMapperProfileBuilder> _builders = [];

    protected virtual void ConfigureInternal(TypeAdapterConfig config)
    {
    }

    protected AutoMapperProfileBuilder<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var builder = new AutoMapperProfileBuilder<TSource, TDestination>();

        _builders.Add(builder);

        return builder;
    }

    protected AutoMapperProfileBuilderGeneric CreateMap(Type source, Type destination)
    {
        var builder = new AutoMapperProfileBuilderGeneric(source, destination);

        _builders.Add(builder);

        return builder;
    }

    public void Register(TypeAdapterConfig config)
    {
        foreach (var builder in _builders)
        {
            builder.Build(config);
        }

        ConfigureInternal(config);
    }
}