using Avemepls.Core.Mapping;

using Mapster;

namespace Avemepls.Mapster;

public class MapsterAdapter : IMapper
{
    private readonly MapsterMapper.IMapper _mapper;

    public MapsterAdapter(MapsterMapper.IMapper mapper)
    {
        _mapper = mapper;
    }

    public TDest Map<TDest>(object source) => _mapper.Map<TDest>(source);
    public TDest Map<TSrc, TDest>(TSrc source) => _mapper.Map<TSrc, TDest>(source);
    public TDest Map<TSrc, TDest>(TSrc source, TDest destination) => _mapper.Map(source, destination);

    public IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery)
        => sourceQuery.ProjectToType<TDest>(_mapper.Config);

    public IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery, object parameters)
    {
        // does not working, need to dispose context after materializing collection
        using var scope = new MapContextScope();
        var properties = parameters.GetType().GetProperties();

        foreach (var property in properties)
        {
            scope.Context.Parameters[property.Name] = property.GetValue(parameters)!;
        }

        return sourceQuery.ProjectToType<TDest>(_mapper.Config);
    }
}