using System.Reflection;

using Avemepls.Tests.Architecture;

namespace Tests.Architecture;

public class DependencyTests : DependencyTestsBase
{
    protected override Assembly[] GetAssemblies() => [
        .. AppDomain.CurrentDomain.GetAssemblies()
            .Where(x =>
            {
                var name = x.GetName().Name!;

                return name.StartsWith("Schedia", StringComparison.InvariantCulture)
                       || name.StartsWith("Avemepls", StringComparison.InvariantCulture);
            })
    ];
}