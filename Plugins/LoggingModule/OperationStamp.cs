using ModuleContracts;

namespace LoggingModule;

public sealed class OperationStamp : IOperationStamp
{
    public Guid Id { get; } = Guid.NewGuid();
}
