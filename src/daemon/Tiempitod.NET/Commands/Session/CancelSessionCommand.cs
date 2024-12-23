namespace Tiempitod.NET.Commands.Session;

/// <summary>
/// Command that cancels the current active session.
/// </summary>
public class CancelSessionCommand : ICommand
{
    private readonly CancellationTokenSource _cancellationTokenSource;
   
    public CancelSessionCommand(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;
    }
   
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await _cancellationTokenSource.CancelAsync();
    }
}
