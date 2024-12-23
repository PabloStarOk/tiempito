namespace Tiempitod.NET.Commands;

public interface ICommandHandler: IDisposable
{
    public Task HandleCommandAsync(string commandString, CancellationToken stoppingToken);
}
