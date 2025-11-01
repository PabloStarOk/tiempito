using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles the enabling/disabling of a configuration feature via the <see cref="IUserConfigService"/>.
/// </summary>
/// <param name="userConfigService">Service for user configuration operations.</param>
internal sealed class UserFeatureConfigCommandHandler(IUserConfigService userConfigService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is UserFeatureConfigCommand;

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not UserFeatureConfigCommand featCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(UserFeatureConfigCommand)}.", nameof(command));
        }

        return featCmd.Enable
            ? await userConfigService.EnableFeatureAsync(featCmd.Feature)
            : await userConfigService.DisableFeatureAsync(featCmd.Feature);
    }
}