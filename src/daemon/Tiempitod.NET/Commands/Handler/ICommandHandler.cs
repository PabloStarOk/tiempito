namespace Tiempitod.NET.Commands.Handler;

public interface ICommandHandler
{
    public Task HandleCommandAsync(string commandString);
}
