using System.Reflection;

namespace Avemepls.Infrastructure.Enums;

public class EnumsServiceOptions
{
    public List<string> ExcludingAssemblies { get; } = [];

    public List<Assembly> Assemblies { get; } = [];

    public EnumsServiceOptions Exclude(params string[] excludingAssemblies)
    {
        foreach (var excludingAssembly in excludingAssemblies)
        {
            ExcludingAssemblies.Add(excludingAssembly);
        }

        return this;
    }

    public EnumsServiceOptions UseAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Assemblies.Add(assembly);
        }

        return this;
    }
}