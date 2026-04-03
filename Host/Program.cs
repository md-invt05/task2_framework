using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleContracts;
using ModuleEngine;

Console.OutputEncoding = Encoding.UTF8;

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .Build();

    var enabledModules = configuration.GetSection("Modules").Get<string[]>() ?? [];
    var modulesPath = Path.Combine(AppContext.BaseDirectory, "modules");

    var loader = new ModuleLoader();
    var catalog = new ModuleCatalog();
    var runtime = new ModuleRuntime();

    var discoveredModules = loader.LoadFromDirectory(modulesPath);
    var executionPlan = catalog.BuildExecutionPlan(discoveredModules, enabledModules);

    Console.WriteLine("Порядок запуска модулей:");

    for (var index = 0; index < executionPlan.Count; index++)
    {
        Console.WriteLine($"{index + 1}. {executionPlan[index].Name}");
    }

    using var serviceProvider = runtime.BuildServiceProvider(executionPlan);
    await runtime.InitializeModulesAsync(executionPlan, serviceProvider, cancellationTokenSource.Token);

    var actions = serviceProvider.GetServices<IAppAction>().ToList();

    Console.WriteLine();
    Console.WriteLine("Демонстрационные действия:");

    if (actions.Count == 0)
    {
        Console.WriteLine("Доступных действий нет.");
    }

    foreach (var action in actions)
    {
        Console.WriteLine();
        Console.WriteLine($"[{action.Title}]");
        await action.ExecuteAsync(cancellationTokenSource.Token);
    }

    Console.WriteLine();
    Console.WriteLine("Работа завершена.");
}
catch (ModuleLoadException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Выполнение остановлено пользователем.");
    Environment.ExitCode = 2;
}
