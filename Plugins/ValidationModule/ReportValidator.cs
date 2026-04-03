using ModuleContracts;

namespace ValidationModule;

public sealed class ReportValidator : IReportValidator
{
    private readonly IAppLogger _logger;

    public ReportValidator(IAppLogger logger)
    {
        _logger = logger;
    }

    public ReportValidationResult Validate(IReadOnlyCollection<ReportRecord> records)
    {
        var validRecords = new List<ReportRecord>();
        var errors = new List<string>();

        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.Customer))
            {
                errors.Add($"Запись #{record.Id}: не указано имя клиента.");
                continue;
            }

            if (record.Amount <= 0)
            {
                errors.Add($"Запись #{record.Id}: сумма должна быть больше нуля.");
                continue;
            }

            validRecords.Add(record);
        }

        _logger.Log($"Проверка завершена. Корректных записей: {validRecords.Count}, ошибок: {errors.Count}.");
        return new ReportValidationResult(validRecords, errors);
    }
}
