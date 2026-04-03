namespace ModuleContracts;

public interface IReportDataProvider
{
    IReadOnlyCollection<ReportRecord> GetRecords();
}
