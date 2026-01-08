using System.Reflection;

namespace Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

public static class BlazorAssembliesRegistry
{
    internal static ICollection<Assembly> AssembliesList { get; } = new HashSet<Assembly>();

    public static IEnumerable<Assembly> Assemblies => AssembliesList;
}