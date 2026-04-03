namespace ModuleContracts;

public interface IAppAction
{
    string Title { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
