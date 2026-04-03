using ModuleContracts;

namespace ExportModule;

public sealed class FileReportExporter : IReportExporter
{
    private readonly IAppLogger _logger;

    public FileReportExporter(IAppLogger logger)
    {
        _logger = logger;
    }

    public async Task<string> ExportAsync(string content, CancellationToken cancellationToken)
    {
        var exportDirectory = Path.Combine(AppContext.BaseDirectory, "exports");
        Directory.CreateDirectory(exportDirectory);

        var fileName = $"report-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        var filePath = Path.Combine(exportDirectory, fileName);

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.Log($"Отчет сохранен в файл: {filePath}");

        return filePath;
    }
}
