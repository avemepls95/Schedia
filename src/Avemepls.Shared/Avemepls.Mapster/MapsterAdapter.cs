using Avemepls.Core.Mapping;

using Mapster;

namespace Avemepls.Mapster;

public class MapsterAdapter(MapsterMapper.IMapper mapper) : IMapper
{
    public TDest Map<TDest>(object source) => mapper.Map<TDest>(source);
    public TDest Map<TSrc, TDest>(TSrc source) => mapper.Map<TSrc, TDest>(source);
    public TDest Map<TSrc, TDest>(TSrc source, TDest destination) => mapper.Map(source, destination);

    public IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery)
        => sourceQuery.ProjectToType<TDest>(mapper.Config);

    public IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery, object parameters)
    {
        // does not working, need to dispose context after materializing collection
        using var scope = new MapContextScope();
        var properties = parameters.GetType().GetProperties();

        foreach (var property in properties)
        {
            scope.Context.Parameters[property.Name] = property.GetValue(parameters)!;
        }

        return sourceQuery.ProjectToType<TDest>(mapper.Config);
    }
}