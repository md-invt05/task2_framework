using ModuleContracts;

namespace LoggingModule;

public sealed class ConsoleAppLogger : IAppLogger
{
    private readonly IAppSession _appSession;

    public ConsoleAppLogger(IAppSession appSession)
    {
        _appSession = appSession;
    }

    public void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Session:{_appSession.Id.ToString("N")[..8]}] {message}");
    }
}
