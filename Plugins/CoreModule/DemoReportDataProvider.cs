using ModuleContracts;

namespace CoreModule;

public sealed class DemoReportDataProvider : IReportDataProvider
{
    public IReadOnlyCollection<ReportRecord> GetRecords()
    {
        return
        [
            new ReportRecord(1, "Contoso", 1200m),
            new ReportRecord(2, "", 250m),
            new ReportRecord(3, "Northwind", -50m),
            new ReportRecord(4, "Adventure Works", 780m)
        ];
    }
}
