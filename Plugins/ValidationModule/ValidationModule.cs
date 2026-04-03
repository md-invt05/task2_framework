using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace ValidationModule;

public sealed class ValidationModule : IAppModule
{
    public string Name => "Validation";

    public IReadOnlyCollection<string> Requires => ["Core", "Logging"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IReportValidator, ReportValidator>();
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.Log("Модуль Validation инициализирован.");
        return Task.CompletedTask;
    }
}
