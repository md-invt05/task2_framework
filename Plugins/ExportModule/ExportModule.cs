using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace ExportModule;

public sealed class ExportModule : IAppModule
{
    public string Name => "Export";

    public IReadOnlyCollection<string> Requires => ["Logging"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IReportExporter, FileReportExporter>();
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.Log("Модуль Export инициализирован.");
        return Task.CompletedTask;
    }
}
