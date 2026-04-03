using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;

namespace ModuleEngine;

public sealed class ModuleRuntime
{
    public ServiceProvider BuildServiceProvider(IEnumerable<IAppModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        var services = new ServiceCollection();

        foreach (var module in modules)
        {
            module.RegisterServices(services);
        }

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    public async Task InitializeModulesAsync(
        IEnumerable<IAppModule> modules,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        foreach (var module in modules)
        {
            await module.InitializeAsync(serviceProvider, cancellationToken);
        }
    }
}
