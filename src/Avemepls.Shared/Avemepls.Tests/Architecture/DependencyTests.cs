using System.Reflection;

using Xunit;

namespace Avemepls.Tests.Architecture;

public abstract class DependencyTestsBase
{
    /// <summary>
    /// Сборки для анализа
    /// </summary>
    protected abstract Assembly[] GetAssemblies();

    /// <summary>
    /// Исключения для source-сборок: сборки, которые не нужно анализировать как источники зависимостей
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// protected override HashSet<string> GetSourceAssemblyExclusions()
    /// {
    ///     return new HashSet<string>
    ///     {
    ///         "MyProject.DataAccess.Legacy"
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    protected virtual HashSet<string> GetSourceAssemblyExclusions() => [];

    /// <summary>
    /// Исключения для source-слоев: паттерны слоев, которые не нужно анализировать как источники
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// protected override HashSet<string/> GetSourceLayerExclusions()
    /// {
    ///     return new HashSet<string/>
    ///     {
    ///         ".Domain"
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    protected virtual HashSet<string> GetSourceLayerExclusions() => [];

    /// <summary>
    /// Исключения для forbidden-сборок: зависимости на эти сборки игнорируются
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// protected override HashSet<string/> GetForbiddenAssemblyExclusions()
    /// {
    ///     return new HashSet<string/>
    ///     {
    ///         "MyProject.Domain.Shared"
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    protected virtual HashSet<string> GetForbiddenAssemblyExclusions() => [];

    /// <summary>
    /// Исключения для forbidden-слоев: зависимости на эти слои игнорируются
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// protected override HashSet<string/> GetForbiddenLayerExclusions()
    /// {
    ///     return new HashSet<string/>
    ///     {
    ///         ".Domain.Abstractions"
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    protected virtual HashSet<string> GetForbiddenLayerExclusions() => [];

    /// <summary>
    /// Специфические исключения: пары (sourceAssembly -> forbiddenAssembly) для точечных исключений
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// protected override Dictionary<string, HashSet<string>> GetSpecificExclusions()
    /// {
    ///     return new Dictionary<string, HashSet<string>>
    ///     {
    ///         // MyProject.DataAccess.Reporting может зависеть от специфичных сборок домена
    ///         ["MyProject.DataAccess.Reporting"] = new HashSet<string>
    ///         {
    ///             "MyProject.Domain.Reports",
    ///             "MyProject.Domain.Analytics"
    ///         },
    ///         // MyProject.DataAccess.Integration имеет исключения
    ///         ["MyProject.DataAccess.Integration"] = new HashSet<string>
    ///         {
    ///             "MyProject.Application.Contracts"
    ///         }
    ///     };
    /// }
    /// ]]></code>
    /// </example>
    protected virtual Dictionary<string, HashSet<string>> GetSpecificDependencyExclusions() => new();

    protected virtual (string, string, string?)[] GetCustomForbiddenDependencies() => [];

    public static TheoryData<string, string, string> BaseForbiddenDependencies => new()
    {
        { ".DataAccess", ".Domain", "DataAccess can't depend on Domain layer" },
        { ".DataAccess", ".Application", "DataAccess can't depend on Application layer" },
        { ".DataAccess", ".Web", "DataAccess can't depend on Web layer" },
        { ".Domain", ".Application", "Domain can't depend on Application layer" },
        { ".Domain", ".Web", "Domain can't depend on Web layer" },
        { ".Domain", ".Domain", "Domain can't depend on Domain layer" },
        { ".Abstractions", ".DataAccess", "Abstractions can't depend on DataAccess layer" },
        { ".Abstractions", ".Domain", "Abstractions can't depend on Domain layer" },
        { ".Abstractions", ".Application", "Abstractions can't depend on Application layer" },
        { ".Abstractions", ".Web", "Abstractions can't depend on Web layer" }
    };

    [Theory]
    [MemberData(nameof(BaseForbiddenDependencies))]
    public void Forbidden_Dependencies(string sourceLayerPattern, string forbiddenLayerPattern, string because)
        => CheckDependency(sourceLayerPattern, forbiddenLayerPattern, because);

    [Fact]
    public void Custom_Forbidden_Dependencies()
    {
        foreach (var (sourceLayerPattern, forbiddenLayerPattern, because) in GetCustomForbiddenDependencies())
        {
            CheckDependency(
                sourceLayerPattern,
                forbiddenLayerPattern,
                because ?? $"{sourceLayerPattern} can't depend on {forbiddenLayerPattern}");
        }
    }

    private void CheckDependency(string sourceLayerPattern, string forbiddenLayerPattern, string because)
    {
        // Получаем исключения
        var sourceAssemblyExclusions = GetSourceAssemblyExclusions();
        var sourceLayerExclusions = GetSourceLayerExclusions();
        var forbiddenAssemblyExclusions = GetForbiddenAssemblyExclusions();
        var forbiddenLayerExclusions = GetForbiddenLayerExclusions();
        var specificExclusions = GetSpecificDependencyExclusions();

        var sourceAssemblies = GetAssemblies()
            .Where(x =>
            {
                var assemblyName = x.GetName().Name!;

                // Проверяем, что сборка соответствует паттерну source-слоя
                if (!assemblyName.Contains(sourceLayerPattern, StringComparison.InvariantCulture)

                    // Исключаем сборки из списка исключений
                    || sourceAssemblyExclusions.Contains(assemblyName)

                    // Исключаем сборки, содержащие паттерны исключенных слоев
                    || sourceLayerExclusions.Any(exclusion => assemblyName.Contains(exclusion, StringComparison.InvariantCulture)))
                {
                    return false;
                }

                return true;
            });

        var violations = new List<string>();

        // Act
        foreach (var assembly in sourceAssemblies)
        {
            var assemblyName = assembly.GetName().Name!;
            var dependencies = assembly.GetReferencedAssemblies();

            var forbiddenDependencies = dependencies
                .Where(x =>
                {
                    var dependencyName = x.Name!;

                    // Проверяем, что зависимость соответствует паттерну forbidden-слоя
                    if (!dependencyName.Contains(forbiddenLayerPattern, StringComparison.InvariantCulture)

                        // Исключаем сборки из списка forbidden-исключений
                        || forbiddenAssemblyExclusions.Contains(dependencyName)

                        // Исключаем сборки, содержащие паттерны исключенных forbidden-слоев
                        || forbiddenLayerExclusions.Any(exclusion => dependencyName.Contains(exclusion, StringComparison.InvariantCulture))

                        // Проверяем специфические исключения для данной сборки
                        || (specificExclusions.TryGetValue(assemblyName, out var excludedDeps) && excludedDeps.Contains(dependencyName)))
                    {
                        return false;
                    }

                    return true;
                })
                .ToArray();

            if (forbiddenDependencies.Length == 0)
            {
                continue;
            }

            var forbiddenDependenciesAsString = string.Join(", ", forbiddenDependencies.Select(x => $"'{x.Name}'"));
            var message = $"Assembly '{assemblyName}' has dependencies on {forbiddenDependenciesAsString}";
            violations.Add(message);
        }

        // Assert
        var errorMessage = string.Concat(because, ":", Environment.NewLine, string.Join(Environment.NewLine, violations));
        Assert.True(violations.Count == 0, errorMessage);
    }
}