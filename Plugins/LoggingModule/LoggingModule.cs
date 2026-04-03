using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace LoggingModule;

public sealed class LoggingModule : IAppModule
{
    public string Name => "Logging";

    public IReadOnlyCollection<string> Requires => ["Core"];

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLogger, ConsoleAppLogger>();
        services.AddTransient<IOperationStamp, OperationStamp>();
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.Log("Модуль Logging инициализирован.");
        return Task.CompletedTask;
    }
}
