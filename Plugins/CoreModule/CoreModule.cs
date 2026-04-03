using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace CoreModule;

public sealed class CoreModule : IAppModule
{
    public string Name => "Core";

    public IReadOnlyCollection<string> Requires => Array.Empty<string>();

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IReportDataProvider, DemoReportDataProvider>();
        services.AddSingleton<IAppSession, AppSession>();
        services.AddTransient<IAppAction, PreviewDataAction>();
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
