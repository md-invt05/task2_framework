using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;
using ModuleEngine;

namespace Tests;

public sealed class ModuleRuntimeTests
{
    [Fact]
    public async Task BuildServiceProvider_ShouldInjectDependenciesFromContainer()
    {
        var runtime = new ModuleRuntime();
        var modules = new IAppModule[]
        {
            new ContainerTestModule()
        };

        using var serviceProvider = runtime.BuildServiceProvider(modules);
        await runtime.InitializeModulesAsync(modules, serviceProvider, CancellationToken.None);

        var marker = serviceProvider.GetRequiredService<DependencyMarker>();
        var action = Assert.Single(serviceProvider.GetServices<IAppAction>());

        var containerAwareAction = Assert.IsType<ContainerAwareAction>(action);
        Assert.Same(marker, containerAwareAction.Marker);
    }

    [Fact]
    public async Task BuildServiceProvider_ShouldRespectSingletonAndTransientLifetimes()
    {
        var exportDirectory = Path.Combine(AppContext.BaseDirectory, "exports");

        if (Directory.Exists(exportDirectory))
        {
            Directory.Delete(exportDirectory, recursive: true);
        }

        var modules = new IAppModule[]
        {
            new CoreModule.CoreModule(),
            new LoggingModule.LoggingModule(),
            new ValidationModule.ValidationModule(),
            new ExportModule.ExportModule(),
            new ReportModule.ReportModule()
        };

        var catalog = new ModuleCatalog();
        var runtime = new ModuleRuntime();
        var executionPlan = catalog.BuildExecutionPlan(modules, ["Core", "Logging", "Validation", "Export", "Report"]);

        using var serviceProvider = runtime.BuildServiceProvider(executionPlan);
        await runtime.InitializeModulesAsync(executionPlan, serviceProvider, CancellationToken.None);

        var session1 = serviceProvider.GetRequiredService<IAppSession>();
        var session2 = serviceProvider.GetRequiredService<IAppSession>();
        var stamp1 = serviceProvider.GetRequiredService<IOperationStamp>();
        var stamp2 = serviceProvider.GetRequiredService<IOperationStamp>();

        Assert.Same(session1, session2);
        Assert.NotEqual(stamp1.Id, stamp2.Id);

        var reportAction = serviceProvider
            .GetServices<IAppAction>()
            .Single(action => action.Title.Contains("отчет", StringComparison.OrdinalIgnoreCase));

        await reportAction.ExecuteAsync(CancellationToken.None);

        Assert.True(Directory.Exists(exportDirectory));
        Assert.NotEmpty(Directory.GetFiles(exportDirectory, "*.txt"));
    }

    private sealed class ContainerTestModule : IAppModule
    {
        public string Name => "Container";

        public IReadOnlyCollection<string> Requires => Array.Empty<string>();

        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<DependencyMarker>();
            services.AddTransient<IAppAction, ContainerAwareAction>();
        }

        public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public sealed class DependencyMarker
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public sealed class ContainerAwareAction : IAppAction
    {
        public ContainerAwareAction(DependencyMarker marker)
        {
            Marker = marker;
        }

        public DependencyMarker Marker { get; }

        public string Title => "Container action";

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
