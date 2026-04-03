using ModuleContracts;

namespace CoreModule;

public sealed class PreviewDataAction : IAppAction
{
    private readonly IReportDataProvider _reportDataProvider;
    private readonly IAppSession _appSession;

    public PreviewDataAction(IReportDataProvider reportDataProvider, IAppSession appSession)
    {
        _reportDataProvider = reportDataProvider;
        _appSession = appSession;
    }

    public string Title => "Просмотр исходных данных";

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Идентификатор singleton-сессии: {_appSession.Id}");

        foreach (var record in _reportDataProvider.GetRecords())
        {
            Console.WriteLine($"Запись #{record.Id}: клиент={record.Customer}, сумма={record.Amount}");
        }

        return Task.CompletedTask;
    }
}
