using Mapster;

namespace Avemepls.Mapster.Configuration;

#pragma warning disable S1694
public abstract class AutoMapperMemberConfigurationExpressionBuilder
#pragma warning restore S1694
{
    internal abstract void Build(TypeAdapterSetter config);
}