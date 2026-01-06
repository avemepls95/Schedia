namespace Avemepls.Core.Mapping;

public interface IMapper
{
    TDest Map<TDest>(object source);

    TDest Map<TSrc, TDest>(TSrc source);

    TDest Map<TSrc, TDest>(TSrc source, TDest destination);

    IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery);

    IQueryable<TDest> ProjectTo<TDest>(IQueryable sourceQuery, object parameters);
}