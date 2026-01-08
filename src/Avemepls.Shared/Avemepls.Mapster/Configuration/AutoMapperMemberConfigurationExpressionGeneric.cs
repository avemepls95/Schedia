using Mapster;

namespace Avemepls.Mapster.Configuration;

public class AutoMapperMemberConfigurationExpressionGeneric : AutoMapperMemberConfigurationExpressionBuilder
{
    private readonly string _destinationMember;
    private readonly List<Action<TypeAdapterSetter>> _configurators = [];

    public AutoMapperMemberConfigurationExpressionGeneric(string destinationMember)
    {
        _destinationMember = destinationMember;
    }

    public AutoMapperMemberConfigurationExpressionGeneric Ignore()
    {
        _configurators.Add(config => config.Ignore(_destinationMember));

        return this;
    }

    internal override void Build(TypeAdapterSetter config)
    {
        foreach (var configurator in _configurators)
        {
            configurator.Invoke(config);
        }
    }
}