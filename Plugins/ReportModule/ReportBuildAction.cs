using System.Text;
using ModuleContracts;

namespace ReportModule;

public sealed class ReportBuildAction : IAppAction
{
    private readonly IReportDataProvider _reportDataProvider;
    private readonly IReportValidator _reportValidator;
    private readonly IReportExporter _reportExporter;
    private readonly IAppLogger _logger;
    private readonly IAppSession _appSession;
    private readonly IOperationStamp _firstOperationStamp;
    private readonly IOperationStamp _secondOperationStamp;

    public ReportBuildAction(
        IReportDataProvider reportDataProvider,
        IReportValidator reportValidator,
        IReportExporter reportExporter,
        IAppLogger logger,
        IAppSession appSession,
        IOperationStamp firstOperationStamp,
        IOperationStamp secondOperationStamp)
    {
        _reportDataProvider = reportDataProvider;
        _reportValidator = reportValidator;
        _reportExporter = reportExporter;
        _logger = logger;
        _appSession = appSession;
        _firstOperationStamp = firstOperationStamp;
        _secondOperationStamp = secondOperationStamp;
    }

    public string Title => "Формирование и экспорт отчета";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var records = _reportDataProvider.GetRecords();
        var validationResult = _reportValidator.Validate(records);
        var totalAmount = validationResult.ValidRecords.Sum(record => record.Amount);

        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("Итоговый отчет");
        reportBuilder.AppendLine($"Сессия приложения (singleton): {_appSession.Id}");
        reportBuilder.AppendLine($"Transient-метка 1: {_firstOperationStamp.Id}");
        reportBuilder.AppendLine($"Transient-метка 2: {_secondOperationStamp.Id}");
        reportBuilder.AppendLine($"Валидных записей: {validationResult.ValidRecords.Count}");
        reportBuilder.AppendLine($"Общая сумма: {totalAmount}");
        reportBuilder.AppendLine("Данные:");

        foreach (var record in validationResult.ValidRecords)
        {
            reportBuilder.AppendLine($"- #{record.Id}: {record.Customer} => {record.Amount}");
        }

        if (validationResult.Errors.Count > 0)
        {
            reportBuilder.AppendLine("Ошибки валидации:");

            foreach (var error in validationResult.Errors)
            {
                reportBuilder.AppendLine($"- {error}");
            }
        }

        var reportContent = reportBuilder.ToString();
        var exportPath = await _reportExporter.ExportAsync(reportContent, cancellationToken);

        _logger.Log($"Отчет сформирован. Валидных записей: {validationResult.ValidRecords.Count}, сумма: {totalAmount}.");
        Console.WriteLine(reportContent);
        Console.WriteLine($"Файл отчета: {exportPath}");
    }
}
