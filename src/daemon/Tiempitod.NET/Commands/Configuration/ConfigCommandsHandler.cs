using Tiempito.IPC.NET.Messages;
using Tiempitod.NET.Configuration.User;
using Tiempitod.NET.Exceptions;

namespace Tiempitod.NET.Commands.Configuration;

// TODO: This class can be refactored along with SessionCommandsHandler to provide only the CreateCommand method.
/// <summary>
/// Handles commands to modify user's configuration/
/// </summary>
public class ConfigCommandsHandler : ICommandHandler
{
    private readonly IUserConfigProvider _userConfigProvider;
    
    /// <summary>
    /// Instantiates a <see cref="ConfigCommandsHandler"/>.
    /// </summary>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    public ConfigCommandsHandler(IUserConfigProvider userConfigProvider)
    {
        _userConfigProvider = userConfigProvider;
    }
    
    public async Task<OperationResult> HandleCommandAsync(Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            ICommand command = CreateCommand(request);
            return await command.ExecuteAsync(cancellationToken);
        }
        catch (CommandNotFoundException ex)
        {
            return new OperationResult
            (
                Success: false,
                Message: ex.Message
            );
        }
    }
    
    /// <summary>
    /// Creates a command with the given command request.
    /// </summary>
    /// <param name="request">Request to create the command.</param>
    /// <returns>An <see cref="ICommand"/>.</returns>
    /// <exception cref="CommandNotFoundException">If the given command is not recognized.</exception>
    private ICommand CreateCommand(Request request)
    {
        IReadOnlyDictionary<string, string> args = request.Arguments;
        
        return request.SubcommandType switch
        {
            "set" => new SetConfigCommand(_userConfigProvider, args),
            "enable" => new EnableConfigCommand(_userConfigProvider, args),
            "disable" => new DisableConfigCommand(_userConfigProvider, args),
            _ => throw new CommandNotFoundException(request.SubcommandType)
        };
    }
}
