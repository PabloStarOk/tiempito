using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles the enabling/disabling of a configuration feature via the <see cref="IUserConfigService"/>.
/// </summary>
/// <param name="userConfigService">Service for user configuration operations.</param>
/// <param name="enable">Indicates whether to enable (`true`) or disable (`false`) the feature.</param>
internal sealed class UserFeatureConfigCommandHandler(IUserConfigService userConfigService, bool enable)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Config), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType is "enable" or "disable";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (!command.Arguments.TryGetValue("feature", out string? feature)
            || string.IsNullOrWhiteSpace(feature))
        {
            return new OperationResult(Success: false, "Feature was not provided.");
        }

        return enable
            ? await userConfigService.EnableFeatureAsync(feature)
            : await userConfigService.DisableFeatureAsync(feature);
    }
}