namespace Tiempitod.NET.Commands;

public interface ICommandHandler
{
    public Task HandleCommandAsync(string commandString);
}
