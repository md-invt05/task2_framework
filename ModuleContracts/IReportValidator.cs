namespace ModuleContracts;

public interface IReportValidator
{
    ReportValidationResult Validate(IReadOnlyCollection<ReportRecord> records);
}
