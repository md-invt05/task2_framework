namespace ModuleContracts;

public interface IReportExporter
{
    Task<string> ExportAsync(string content, CancellationToken cancellationToken);
}
