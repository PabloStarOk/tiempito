using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles the "set" subcommand for configuration commands, updating the default session ID.
/// </summary>
/// <param name="userConfigService">Service for managing user configuration.</param>
internal sealed class SetConfigCommandHandler(IUserConfigService userConfigService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is SetConfigCommand;

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not SetConfigCommand setCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(SetConfigCommand)}.", nameof(command));
        }

        return await userConfigService.ChangeDefaultSessionConfigAsync(setCmd.DefaultSessionConfigId);
    }
}
