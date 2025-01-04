using Tiempito.IPC.NET.Messages;
using Tiempitod.NET.Commands.ConfigCommands;
using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands;

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
        if (!TryCreateCommand(request, out ICommand? command))
        {
            return new OperationResult
            (
                Success: false,
                Message: $"Unknown config command '{request.SubcommandType}'"
            );
        }
        return await command.ExecuteAsync(cancellationToken);
    }
    
    /// <summary>
    /// Tries to create command with the given command request.
    /// </summary>
    /// <param name="request">Request to create the command.</param>
    /// <param name="command">Created command.</param>
    /// <returns>True if the command was created, false otherwise.</returns>
    private bool TryCreateCommand(Request request, out ICommand? command)
    {
        command = null;
        IReadOnlyDictionary<string, string> args = request.Arguments;
        
        command = request.SubcommandType switch
        {
            "set" => new SetConfigCommand(_userConfigProvider, args),
            "enable" => new EnableConfigCommand(_userConfigProvider, args),
            "disable" => new DisableConfigCommand(_userConfigProvider, args),
            _ => null
        };

        return command != null;
    }
}
