using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace ReportModule;

public sealed class ReportModule : IAppModule
{
    public string Name => "Report";

    public IReadOnlyCollection<string> Requires => ["Core", "Logging", "Validation", "Export"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IAppAction, ReportBuildAction>();
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.Log("Модуль Report инициализирован.");
        return Task.CompletedTask;
    }
}
