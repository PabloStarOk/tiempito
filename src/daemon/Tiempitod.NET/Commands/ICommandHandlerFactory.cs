namespace Tiempitod.NET.Commands;

/// <summary>
/// Defines a factory to create command handlers.
/// </summary>
public interface ICommandHandlerFactory
{
    /// <summary>
    /// Creates an <see cref="ICommandHandler"/>.
    /// </summary>
    /// <param name="commandType">Command type the <see cref="ICommandHandler"/> must handle.</param>
    /// <returns>An <see cref="ICommandHandler"/> that can handle the given command type.</returns>
    public ICommandHandler CreateHandler(string commandType);
}
