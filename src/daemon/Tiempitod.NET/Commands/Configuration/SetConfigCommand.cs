using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.User.Interfaces;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
/// <param name="userConfigService">Service of user's configuration.</param>
/// <param name="arguments">Parameters and their values to modify.</param>
public class SetConfigCommand(
    IUserConfigService userConfigService,
    IReadOnlyDictionary<string, string> arguments) : ICommand
{
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("default-session-id", out string? defaultSessionId)
            || string.IsNullOrWhiteSpace(defaultSessionId))
            return new OperationResult(Success: false, Message: "Nothing to update.");

        return await userConfigService.ChangeDefaultSessionConfigAsync(defaultSessionId);
    }
}
