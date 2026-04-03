using ModuleContracts;

namespace CoreModule;

public sealed class AppSession : IAppSession
{
    public Guid Id { get; } = Guid.NewGuid();
}
