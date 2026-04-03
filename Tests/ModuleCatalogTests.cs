using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;
using ModuleEngine;

namespace Tests;

public sealed class ModuleCatalogTests
{
    [Fact]
    public void BuildExecutionPlan_ShouldOrderModulesForDependencyChain()
    {
        var modules = new IAppModule[]
        {
            new TestModule("Report", "Export"),
            new TestModule("Export", "Validation"),
            new TestModule("Validation", "Logging"),
            new TestModule("Logging", "Core"),
            new TestModule("Core")
        };

        var catalog = new ModuleCatalog();

        var executionPlan = catalog.BuildExecutionPlan(modules, ["Report", "Export", "Validation", "Logging", "Core"]);

        Assert.Equal(["Core", "Logging", "Validation", "Export", "Report"], executionPlan.Select(module => module.Name).ToArray());
    }

    [Fact]
    public void BuildExecutionPlan_ShouldKeepDependencyOrderForSeveralBranches()
    {
        var modules = new IAppModule[]
        {
            new TestModule("A"),
            new TestModule("B"),
            new TestModule("C", "A"),
            new TestModule("D", "B", "C")
        };

        var catalog = new ModuleCatalog();

        var executionPlan = catalog.BuildExecutionPlan(modules, ["D", "C", "B", "A"]);
        var orderedNames = executionPlan.Select(module => module.Name).ToArray();

        Assert.True(Array.IndexOf(orderedNames, "A") < Array.IndexOf(orderedNames, "C"));
        Assert.True(Array.IndexOf(orderedNames, "B") < Array.IndexOf(orderedNames, "D"));
        Assert.True(Array.IndexOf(orderedNames, "C") < Array.IndexOf(orderedNames, "D"));
    }

    [Fact]
    public void BuildExecutionPlan_ShouldUseCaseInsensitiveModuleNames()
    {
        var modules = new IAppModule[]
        {
            new TestModule("Core"),
            new TestModule("Report", "core")
        };

        var catalog = new ModuleCatalog();

        var executionPlan = catalog.BuildExecutionPlan(modules, ["report", "CORE"]);

        Assert.Equal(["Core", "Report"], executionPlan.Select(module => module.Name).ToArray());
    }

    [Fact]
    public void BuildExecutionPlan_ShouldThrowReadableError_WhenEnabledModuleIsMissing()
    {
        var modules = new IAppModule[]
        {
            new TestModule("Core")
        };

        var catalog = new ModuleCatalog();

        var exception = Assert.Throws<ModuleLoadException>(() => catalog.BuildExecutionPlan(modules, ["Core", "Report"]));

        Assert.Contains("Модуль не найден", exception.Message);
        Assert.Contains("Report", exception.Message);
    }

    [Fact]
    public void BuildExecutionPlan_ShouldThrowReadableError_WhenDependencyIsMissing()
    {
        var modules = new IAppModule[]
        {
            new TestModule("Core"),
            new TestModule("Report", "Export")
        };

        var catalog = new ModuleCatalog();

        var exception = Assert.Throws<ModuleLoadException>(() => catalog.BuildExecutionPlan(modules, ["Core", "Report"]));

        Assert.Contains("Не хватает модуля для зависимости", exception.Message);
        Assert.Contains("Report", exception.Message);
        Assert.Contains("Export", exception.Message);
    }

    [Fact]
    public void BuildExecutionPlan_ShouldThrowReadableError_WhenCycleDetected()
    {
        var modules = new IAppModule[]
        {
            new TestModule("A", "B"),
            new TestModule("B", "C"),
            new TestModule("C", "A")
        };

        var catalog = new ModuleCatalog();

        var exception = Assert.Throws<ModuleLoadException>(() => catalog.BuildExecutionPlan(modules, ["A", "B", "C"]));

        Assert.Contains("циклическая зависимость", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A", exception.Message);
        Assert.Contains("B", exception.Message);
        Assert.Contains("C", exception.Message);
    }

    private sealed class TestModule : IAppModule
    {
        public TestModule(string name, params string[] requires)
        {
            Name = name;
            Requires = requires;
        }

        public string Name { get; }

        public IReadOnlyCollection<string> Requires { get; }

        public void RegisterServices(IServiceCollection services)
        {
        }

        public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
