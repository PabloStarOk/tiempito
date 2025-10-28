using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles the "set" subcommand for configuration commands, updating the default session ID.
/// </summary>
/// <param name="userConfigService">Service for managing user configuration.</param>
internal sealed class SetConfigCommandHandler(IUserConfigService userConfigService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Config), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "set";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (!command.Arguments.TryGetValue("default-session-id", out string? defaultSessionId)
            || string.IsNullOrWhiteSpace(defaultSessionId))
        {
            return new OperationResult(Success: false, Message: "Nothing to update.");
        }

        return await userConfigService.ChangeDefaultSessionConfigAsync(defaultSessionId);
    }
}
