namespace ModuleContracts;

public sealed record ReportValidationResult(
    IReadOnlyCollection<ReportRecord> ValidRecords,
    IReadOnlyCollection<string> Errors);
